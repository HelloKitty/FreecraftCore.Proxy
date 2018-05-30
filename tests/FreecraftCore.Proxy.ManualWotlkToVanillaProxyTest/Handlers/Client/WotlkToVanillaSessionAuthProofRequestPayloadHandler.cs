using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binarysharp.MemoryManagement;
using Common.Logging;
using FreecraftCore.Crypto;
using GladNet;
using JetBrains.Annotations;
using ICryptoServiceProvider = GladNet.ICryptoServiceProvider;

namespace FreecraftCore
{
	[ClientPayloadHandler]
	public sealed class WotlkToVanillaGameAuthRequestPayloadHandler : BaseGameClientPayloadHandler<SessionAuthProofRequest>
	{
		private ICryptoKeyInitializable<byte[]> CryptoInitializer { get; }

		/// <inheritdoc />
		public WotlkToVanillaGameAuthRequestPayloadHandler([NotNull] ILog logger, [NotNull] ICryptoKeyInitializable<byte[]> cryptoInit)
			: base(logger)
		{
			CryptoInitializer = cryptoInit ?? throw new ArgumentNullException(nameof(cryptoInit));
		}

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
		/// <inheritdoc />
		public override async Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SessionAuthProofRequest payload)
		{
			Logger.Info("About to require session key from client.");

			//Ok, so when the client sends this it is responding to an auth challenge event.
			//The response, whether we were successful or not, will have encrypted headers.
			//We must read the session key from the client now and then
			//we want to initialize our ingoing and outgoing encryption.
			Console.Write("Enter Warcraft ProcessId: ");
			string processId = Console.ReadLine();

			byte[] sessionKey = ReadSessionKeyFromProcessMemory(processId);

			await context.ProxyConnection.SendMessage(new SessionAuthProofRequest_Vanilla(ClientBuild.Vanilla_1_12_1, payload.AccountName, payload.RandomSeedBytes, payload.SessionDigest))
				.ConfigureAwait(false);

			//We should send the auth request before initializing encryption
			CryptoInitializer.Initialize(sessionKey);
		}
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage

		private static byte[] ReadSessionKeyFromProcessMemory(string processId)
		{
			using(MemorySharp memorySharpProvider = new MemorySharp(Int32.Parse(processId)))
			{
				IntPtr sub_6B0970Result = memorySharpProvider.Read<IntPtr>((IntPtr)0x00C79CF4, false);
				IntPtr sessionKeyAddress = sub_6B0970Result + 0x508;
				return memorySharpProvider.Read<byte>(sessionKeyAddress, 40, false);
			}
		}
	}
}