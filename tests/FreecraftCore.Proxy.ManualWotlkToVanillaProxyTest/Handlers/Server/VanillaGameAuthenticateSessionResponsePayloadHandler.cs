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
	[ServerPayloadHandler]
	public sealed class VanillaGameAuthenticateSessionResponsePayloadHandler : BaseGameServerPayloadHandler<AuthenticateSessionResponse>
	{
		private ICryptoKeyInitializable<byte[]> CryptoInitializer { get; }

		/// <inheritdoc />
		public VanillaGameAuthenticateSessionResponsePayloadHandler([NotNull] ILog logger, [NotNull] ICryptoKeyInitializable<byte[]> cryptoInit) 
			: base(logger)
		{
			CryptoInitializer = cryptoInit ?? throw new ArgumentNullException(nameof(cryptoInit));
		}

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
		/// <inheritdoc />
		public override async Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, AuthenticateSessionResponse payload)
		{
			if(Logger.IsInfoEnabled)
				Logger.Info($"Auth Response Result: {payload.AuthenticationResult}");

			await context.ProxyConnection.SendMessageImmediately(payload, DeliveryMethod.ReliableOrdered)
				.ConfigureAwait(false);
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
