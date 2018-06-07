
using System;
using System.Threading;
using System.Threading.Tasks;
using GladNet;
using Reinterpret.Net;

namespace FreecraftCore
{
	public sealed class VanillaWoWClientReadServerWriteProxyPacketPayloadReaderWriterDecorator<TClientType> : WoWClientWriteServerReadProxyPacketPayloadReaderWriterDecorator<TClientType, GamePacketPayload, GamePacketPayload, IGamePacketPayload>
		where TClientType : NetworkClientBase
	{
		/// <inheritdoc />
		public VanillaWoWClientReadServerWriteProxyPacketPayloadReaderWriterDecorator(TClientType decoratedClient, INetworkSerializationService serializer, ICombinedSessionPacketCryptoService cryptoService, int payloadBufferSize = 30000) 
			: base(decoratedClient, serializer, cryptoService, payloadBufferSize)
		{

		}

		protected override async Task<IPacketHeader> BuildHeaderWithDecryption(CancellationToken token)
		{
			//Unlike 3.3.5 the 1.12.1 server header is simplier
			//It is just 2 byte size and 2 byte opcode.
			//But the caller really only wants size. Not opcode
			await ReadAsync(PacketPayloadReadBuffer, 0, 2, token)
				.ConfigureAwait(false);

			if(CryptoService.isInitialized)
				CryptoService.DecryptionService.ProcessBytes(PacketPayloadReadBuffer, 0, 2, PacketPayloadReadBuffer, 0);

			//It's big endian though so we need to reverse it
			//it is safe to do this hack because it will be overridden when more data is read.
			PacketPayloadReadBuffer[2] = PacketPayloadReadBuffer[0];

			ushort size = PacketPayloadReadBuffer.Reinterpret<ushort>(1);

			Console.WriteLine($"Recieved server packet header size: {size}");

			return new ServerPacketHeader(size);
		}
	}
}