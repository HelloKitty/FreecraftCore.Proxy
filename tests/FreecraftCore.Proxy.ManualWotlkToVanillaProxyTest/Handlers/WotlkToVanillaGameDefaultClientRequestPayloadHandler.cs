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
				NetworkOperationCode.CMSG_WARDEN_DATA,

				//TODO: We should implement some of these
				NetworkOperationCode.CMSG_SET_ACTIONBAR_TOGGLES,
				NetworkOperationCode.CMSG_REQUEST_RAID_INFO,
				NetworkOperationCode.CMSG_GMTICKET_GETTICKET,
				NetworkOperationCode.MSG_QUERY_NEXT_MAIL_TIME,
				NetworkOperationCode.CMSG_BATTLEFIELD_STATUS,
				NetworkOperationCode.CMSG_LFG_GET_STATUS,
				NetworkOperationCode.CMSG_LFD_PLAYER_LOCK_INFO_REQUEST,
				NetworkOperationCode.MSG_GUILD_BANK_MONEY_WITHDRAWN,
				NetworkOperationCode.CMSG_CALENDAR_GET_NUM_PENDING,
				NetworkOperationCode.CMSG_VOICE_SESSION_ENABLE,
				
				//Required to recieve updates about the world
				//NetworkOperationCode.CMSG_ZONEUPDATE,
				NetworkOperationCode.CMSG_SET_ACTIVE_VOICE_CHANNEL,

				//TODO: This structure is not the same. We need to it for 1.12.1
				NetworkOperationCode.CMSG_ITEM_QUERY_SINGLE,

				NetworkOperationCode.CMSG_QUESTGIVER_STATUS_QUERY,

				NetworkOperationCode.CMSG_TIME_SYNC_RESP
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
				if(Logger.IsWarnEnabled)
					Logger.Warn($"Discarding based on OpCode: {payload.GetOperationCode()} from client.");
				return;
			}

			//Forward to the server
			await context.ProxyConnection.SendMessage(payload)
				.ConfigureAwait(false);
		}
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
	}
}
