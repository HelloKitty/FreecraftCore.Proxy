using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GladNet;
using Nito.AsyncEx;
using Reinterpret.Net;

namespace FreecraftCore
{
	/// <summary>
	/// Decorator that decorates the provided <see cref="NetworkClientBase"/> with functionality
	/// that allows you to write <see cref="TWritePayloadBaseType"/> directly into the stream/client.
	/// Overloads the usage of <see cref="Write"/> to accomplish this.
	/// </summary>
	/// <typeparam name="TClientType">The type of decorated client.</typeparam>
	/// <typeparam name="TWritePayloadBaseType"></typeparam>
	/// <typeparam name="TReadPayloadBaseType"></typeparam>
	/// <typeparam name="TPayloadConstraintType">The constraint requirement for </typeparam>
	public sealed class WoWClientReadServerWriteProxyPacketPayloadReaderWriterDecorator<TClientType, TReadPayloadBaseType, TWritePayloadBaseType, TPayloadConstraintType> : NetworkClientBase,
		INetworkMessageClient<TReadPayloadBaseType, TWritePayloadBaseType>
		where TClientType : NetworkClientBase
		where TReadPayloadBaseType : class, TPayloadConstraintType
		where TWritePayloadBaseType : class, TPayloadConstraintType
	{
		/// <summary>
		/// The decorated client.
		/// </summary>
		private TClientType DecoratedClient { get; }

		/// <summary>
		/// Service that readers and writers packet headers.
		/// </summary>

		/// <summary>
		/// The serializer service.
		/// </summary>
		private INetworkSerializationService Serializer { get; }

		public ICombinedSessionPacketCryptoService CryptoService { get; }

		/// <summary>
		/// Thread specific buffer used to deserialize the packet header bytes into.
		/// </summary>
		private byte[] PacketPayloadReadBuffer { get; }

		/// <summary>
		/// Async read syncronization object.
		/// </summary>
		private readonly AsyncLock readSynObj = new AsyncLock();

		/// <summary>
		/// Async write syncronization object.
		/// </summary>
		private readonly AsyncLock writeSynObj = new AsyncLock();

		public WoWClientReadServerWriteProxyPacketPayloadReaderWriterDecorator(TClientType decoratedClient, INetworkSerializationService serializer, ICombinedSessionPacketCryptoService cryptoService, int payloadBufferSize = 30000)
		{
			if(decoratedClient == null) throw new ArgumentNullException(nameof(decoratedClient));
			if(serializer == null) throw new ArgumentNullException(nameof(serializer));
			if(payloadBufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(payloadBufferSize));

			DecoratedClient = decoratedClient;
			Serializer = serializer;
			CryptoService = cryptoService;

			//One of the lobby packets is 14,000 bytes. We may even need bigger.
			PacketPayloadReadBuffer = new byte[payloadBufferSize]; //TODO: Do we need a larger buffer for any packets?
		}

		/// <inheritdoc />
		public override Task<bool> ConnectAsync(string address, int port)
		{
			return DecoratedClient.ConnectAsync(address, port);
		}

		/// <inheritdoc />
		public override async Task ClearReadBuffers()
		{
			using(await readSynObj.LockAsync().ConfigureAwait(false))
				await DecoratedClient.ClearReadBuffers()
					.ConfigureAwait(false);
		}

		/// <inheritdoc />
		public override Task DisconnectAsync(int delay)
		{
			return DecoratedClient.DisconnectAsync(delay);
		}

		/// <inheritdoc />
		public void Write(TWritePayloadBaseType payload)
		{
			//Write the outgoing message, it will internally create the header and it will be serialized
			WriteAsync(payload).Wait();
		}

		/// <inheritdoc />
		public override Task WriteAsync(byte[] bytes, int offset, int count)
		{
			return DecoratedClient.WriteAsync(bytes, offset, count);
		}

		/// <inheritdoc />
		public async Task WriteAsync(TWritePayloadBaseType payload)
		{
			Console.WriteLine($"About to send payload to the client.");

			try
			{
				//Serializer the payload first so we can build the header
				byte[] payloadData = Serializer.Serialize(payload);

				//Don't add 2 to the payload length since it contains the 2 byte opcode already
				ServerPacketHeader header = new ServerPacketHeader(payloadData.Length);
				byte[] serverPacketHeader = Serializer.Serialize(header);

				//VERY critical we lock here otherwise we could write a header and then another unrelated body could be written inbetween
				using(await writeSynObj.LockAsync().ConfigureAwait(false))
				{
					//We should check crypto first. If it's init then we need to encrypt the serverPacketHeader
					if(CryptoService.isInitialized)
					{
						CryptoService.EncryptionService.ProcessBytes(serverPacketHeader, 0, serverPacketHeader.Length, serverPacketHeader, 0);

						//Encrypt the opcode on the payload data too
						CryptoService.EncryptionService.ProcessBytes(payloadData, 0, 2, payloadData, 0);
					}

					//It's important to always write the header first
					await DecoratedClient.WriteAsync(serverPacketHeader)
						.ConfigureAwait(false);

					await DecoratedClient.WriteAsync(payloadData, 0, payloadData.Length)
						.ConfigureAwait(false);
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		/// <inheritdoc />
		public override Task<int> ReadAsync(byte[] buffer, int start, int count, CancellationToken token)
		{
			return DecoratedClient.ReadAsync(buffer, start, count, token);
		}

		public async Task<NetworkIncomingMessage<TReadPayloadBaseType>> ReadAsync(CancellationToken token)
		{
			IPacketHeader header = null;
			TReadPayloadBaseType payload = null;

			using(await readSynObj.LockAsync(token).ConfigureAwait(false))
			{
				await ReadAsync(PacketPayloadReadBuffer, 0, 6, token)
					.ConfigureAwait(false);

				//Check crypto first. We may need to decrypt this header
				if(CryptoService.isInitialized)
					CryptoService.DecryptionService.ProcessBytes(PacketPayloadReadBuffer, 0, 6, PacketPayloadReadBuffer, 0);

				OutgoingClientPacketHeader clientHeader = Serializer.Deserialize<OutgoingClientPacketHeader>(PacketPayloadReadBuffer, 0, 6);

				Console.WriteLine($"New ClientHeader: OpCode: {clientHeader.OperationCode} PacketSize: {clientHeader.PacketSize} PayloadSize: {clientHeader.PayloadSize}");

				header = clientHeader;

				Console.WriteLine($"Recieved OpCode: {clientHeader.OperationCode}:{(ushort)clientHeader.OperationCode} from client Encrypted:{CryptoService.isInitialized}");

				//If the header is null it means the socket disconnected
				if(header == null)
					return null;

				//if was canceled the header reading probably returned null anyway
				if(token.IsCancellationRequested)
					return null;

				//We need to start reading at 2 bytes so we can manually insert the opcode
				//into the payload buffer
				await ReadAsync(PacketPayloadReadBuffer, 2, header.PayloadSize, token)
					.ConfigureAwait(false); //TODO: Should we timeout?

				//RACE CONDITION. WE NEED TO LOCK AROUND READING FRM THIS BUFF
				byte[] reinterpretedOpCode = ((short)clientHeader.OperationCode).Reinterpret();

				PacketPayloadReadBuffer[0] = reinterpretedOpCode[0];
				PacketPayloadReadBuffer[1] = reinterpretedOpCode[1];

				//If the token was canceled then the buffer isn't filled and we can't make a message
				if(token.IsCancellationRequested)
					return null;

				//TODO: Revisit thbis to check if +2 is ok
				//Deserialize the bytes starting from the begining but ONLY read up to the payload size. We reuse this buffer and it's large
				//so if we don't specify the length we could end up with an issue.
				//We have to read for and additional two bytes due to the payyload data shifted 2 bytes forward
				payload = Serializer.Deserialize<TReadPayloadBaseType>(PacketPayloadReadBuffer, 0, header.PayloadSize + 2);
			}

			return new NetworkIncomingMessage<TReadPayloadBaseType>(header, payload);
		}
	}
}
