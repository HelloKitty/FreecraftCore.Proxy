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
				NetworkOperationCode.SMSG_FEATURE_SYSTEM_STATUS,

				NetworkOperationCode.SMSG_GUILD_EVENT,
				NetworkOperationCode.SMSG_GUILD_BANK_LIST,
				NetworkOperationCode.SMSG_GUILD_ROSTER,

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
				NetworkOperationCode.SMSG_SET_FORCED_REACTIONS,

				//This packet actually must be implemented properly for the 3.3.5 client
				//it is the most critical packet and it will not go past the loading
				//bar without it.
				NetworkOperationCode.SMSG_AURA_UPDATE_ALL,

				NetworkOperationCode.SMSG_EXPECTED_SPAM_RECORDS,

				//This OpCode is sent by initial before map join
				//but on Cmangos it is called SMSG_SET_REST_START
				NetworkOperationCode.SMSG_QUEST_FORCE_REMOVE,

				//Vanilla 1.12.1 sends IGNORE_LIST opcode as this. So we discard for now
				//Until we can deal with this.
				NetworkOperationCode.CMSG_SET_CONTACT_NOTES,

				NetworkOperationCode.CMSG_WARDEN_DATA,
				NetworkOperationCode.SMSG_WARDEN_DATA,

				//TODO: We should implement. Otherwise we have no binds
				NetworkOperationCode.SMSG_ACTION_BUTTONS,

				//TODO: Research what this is.
				NetworkOperationCode.SMSG_QUEST_FORCE_REMOVE,

				//TODO: Implement so we can get quest responses
				NetworkOperationCode.SMSG_QUESTGIVER_STATUS,

				//TODO: Implement to handle creatures moving around
				NetworkOperationCode.SMSG_MONSTER_MOVE,

				//TODO: Renable when chat transformation is complete
				NetworkOperationCode.SMSG_MESSAGECHAT
			};

		/// <inheritdoc />
		public WotlkToVanillaGameDefaultServerRequestPayloadHandler([NotNull] ILog logger)
			: base(logger)
		{
			//Disable all spell stuff for now.
			foreach(var opCode in Enum.GetNames(typeof(NetworkOperationCode)).Where(s => s.Contains("SPELL")).Select(s => (NetworkOperationCode)Enum.Parse(typeof(NetworkOperationCode), s)))
				this.OpCodeBlackList.Add(opCode);
		}

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
		/// <inheritdoc />
		public override async Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, GamePacketPayload payload)
		{
			if(Logger.IsWarnEnabled)
				Logger.Warn($"Recieved unproxied Payload: {payload.GetOperationCode()}:{payload.GetType().Name}");

			if(payload is UnknownGamePayload)
				return;

			//Since we're connected to a vanilla realm we, at least for now, want to discard unknown opcode payloads
			if((short)payload.GetOperationCode() > 0x41F || OpCodeBlackList.Contains(payload.GetOperationCode()))
			{
				if(Logger.IsWarnEnabled)
					Logger.Warn($"Discarded by OpCode: {payload.GetOperationCode()} from server.");
				return;
			}

			//Forward to the server
			await context.ProxyConnection.SendMessage(payload)
				.ConfigureAwait(false);
		}
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
	}
}
