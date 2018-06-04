using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaGameDefaultClientRequestPayloadHandler : GameDefaultClientRequestHandler
	{
		/// <summary>
		/// List of blacklisted <see cref="NetworkOperationCode"/>s for connecting
		/// from Wotlk to 1.12.1
		/// </summary>
		private HashSet<NetworkOperationCode> OpCodeBlackList { get; }
			= new HashSet<NetworkOperationCode>()
			{
				NetworkOperationCode.CMSG_READY_FOR_ACCOUNT_DATA_TIMES,
				NetworkOperationCode.CMSG_WARDEN_DATA
			};

		/// <inheritdoc />
		public WotlkToVanillaGameDefaultClientRequestPayloadHandler([NotNull] ILog logger)
			: base(logger)
		{

		}

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
		/// <inheritdoc />
		public override async Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, GamePacketPayload payload)
		{
			if(Logger.IsWarnEnabled)
				Logger.Warn($"Recieved unproxied Payload: {payload.GetType().Name} on {this.GetType().Name}");

			if(payload is UnknownGamePayload)
				return;

			//Since we're connected to a vanilla realm we, at least for now, want to discard unknown opcode payloads
			//above CMSG_ACCEPT_LEVEL_GRANT
			if((short)payload.GetOperationCode() > 0x41F || OpCodeBlackList.Contains(payload.GetOperationCode()))
			{
				return;
			}

			//Forward to the server
			await context.ProxyConnection.SendMessage(payload)
				.ConfigureAwait(false);
		}
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
	}
}
