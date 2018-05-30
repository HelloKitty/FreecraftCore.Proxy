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
	//SMSG_LOGIN_VERIFY_WORLD same between 3.3.5 and 1.12.1
	//SMSG_BINDPOINTUPDATE same between 3.3.5 and 1.12.1
	public sealed class WotlkToVanillaGameDefaultServerRequestPayloadHandler : GameDefaultServerResponseHandler
	{
		//TODO: Block SMSG_SET_REST_START, it doesn't exist on 3.3.5 and no opcode for it in Trinitycore
		/// <summary>
		/// List of blacklisted <see cref="NetworkOperationCode"/>s that we should not
		/// send from the server to the wotlk client.
		/// </summary>
		private HashSet<NetworkOperationCode> OpCodeBlackList { get; }
			= new HashSet<NetworkOperationCode>()
			{
				NetworkOperationCode.SMSG_POWER_UPDATE,
				NetworkOperationCode.SMSG_SET_PROFICIENCY,
				NetworkOperationCode.SMSG_SPELL_GO,
				NetworkOperationCode.SMSG_ACCOUNT_DATA_TIMES,
				NetworkOperationCode.SMSG_FEATURE_SYSTEM_STATUS,

				NetworkOperationCode.SMSG_GUILD_EVENT,
				NetworkOperationCode.SMSG_GUILD_BANK_LIST,
				NetworkOperationCode.SMSG_GUILD_ROSTER,
				NetworkOperationCode.SMSG_LEARNED_DANCE_MOVES,

				//We are currently returning an empty list of friends
				//Until we implement this for 3.3.5 and vanilla
				NetworkOperationCode.SMSG_CONTACT_LIST,
				NetworkOperationCode.SMSG_TALENTS_INFO,
				NetworkOperationCode.SMSG_INSTANCE_DIFFICULTY,
				
				//This packet is semi-complex. It will require
				//Some effort to get a 3.3.5 to 1.12.1 compatible
				//implementation. Therefore we will try to avoid this packet for now.
				NetworkOperationCode.SMSG_INITIAL_SPELLS,
				NetworkOperationCode.SMSG_INITIALIZE_FACTIONS,
				NetworkOperationCode.SMSG_SEND_UNLEARN_SPELLS,
				NetworkOperationCode.SMSG_EQUIPMENT_SET_LIST,
				NetworkOperationCode.SMSG_LOGIN_SETTIMESPEED,
				NetworkOperationCode.SMSG_SET_FORCED_REACTIONS,
				NetworkOperationCode.SMSG_COMPRESSED_UPDATE_OBJECT,
				NetworkOperationCode.SMSG_UPDATE_WORLD_STATE,
				NetworkOperationCode.SMSG_TIME_SYNC_REQ,
				NetworkOperationCode.SMSG_AURA_UPDATE_ALL,
				NetworkOperationCode.SMSG_COMPRESSED_MOVES,

				NetworkOperationCode.SMSG_EXPECTED_SPAM_RECORDS,
				NetworkOperationCode.SMSG_MESSAGECHAT,

				//This OpCode is sent by initial before map join
				//but on Cmangos it is called SMSG_SET_REST_START
				NetworkOperationCode.SMSG_QUEST_FORCE_REMOVE,

				//Vanilla 1.12.1 sends IGNORE_LIST opcode as this. So we discard for now
				//Until we can deal with this.
				NetworkOperationCode.CMSG_SET_CONTACT_NOTES
			};

		/// <inheritdoc />
		public WotlkToVanillaGameDefaultServerRequestPayloadHandler([NotNull] ILog logger)
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
			if((short)payload.GetOperationCode() > 0x41F || OpCodeBlackList.Contains(payload.GetOperationCode()))
			{
				Logger.Warn($"Recieved OpCode: {payload.GetOperationCode()} from server. Discared for now because unimplemented or wotlk doesn't support.");
				return;
			}

			//Forward to the server
			await context.ProxyConnection.SendMessage(payload)
				.ConfigureAwait(false);
		}
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
	}
}
