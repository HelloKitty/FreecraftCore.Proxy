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
		/// <inheritdoc />
		public VanillaGameAuthenticateSessionResponsePayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{

		}

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
		/// <inheritdoc />
		public override async Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, AuthenticateSessionResponse payload)
		{
			if(Logger.IsInfoEnabled)
				Logger.Info($"Auth Response Result: {payload.AuthenticationResult}");

			await context.ProxyConnection.SendMessageImmediately(payload, DeliveryMethod.ReliableOrdered)
				.ConfigureAwait(false);


			if(Logger.IsInfoEnabled)
				Logger.Info($"Spoofing addon check information.");

			//After the result is sent for game auth then the client will
			//expect that we send addon information otherwise addons won't work
			//We must send the 
			//Make sure to send the client, not sever by accident
			await context.ProxyConnection.SendMessage(new SMSG_ADDON_INFO_Payload(Enumerable.Repeat(new AddonChecksumResult(0, false), 23).ToArray()))
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
