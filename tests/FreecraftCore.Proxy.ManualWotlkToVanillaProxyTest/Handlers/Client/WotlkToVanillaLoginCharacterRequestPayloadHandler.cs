using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binarysharp.MemoryManagement;
using Common.Logging;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// The wotlk client sends this when it wants to enter the world.
	/// The main issue is 1.12.1 does not send all the packets the wotlk
	/// expects so we spoof them.
	/// </summary>
	[ClientPayloadHandler]
	public sealed class WotlkToVanillaLoginCharacterRequestPayloadHandler : BaseGameClientPayloadHandler<CharacterLoginRequest>
	{
		/// <inheritdoc />
		public WotlkToVanillaLoginCharacterRequestPayloadHandler([NotNull] ILog logger)
			: base(logger)
		{
			
		}

		/// <inheritdoc />
		public override async Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, CharacterLoginRequest payload)
		{
			//TODO: May have to implement SendAccountDataTimes
			//Wotlk expects a lot of extra packets to be sent that 1.12.1 doesn't send
			//Forward the login request after these

			//3.3.5 TC sends in the following order for minimum login
			//Dungeon_Difficulty
			//Login_Verify_World
			//SMSG_MOTD
			//Guild stuff
			//Learned_Dance_Moves
			//SMSG_COMPRESSED_UPDATE_OBJECT
			//SMSG_COMPRESSED_UPDATE_OBJECT
			//CMSG_PLAYED_TIME

			//await context.PayloadSendService.SendMessage(new SMSG_INSTANCE_DIFFICULTY_PAYLOAD());
			await context.PayloadSendService.SendMessage(new SMSG_FEATURE_SYSTEM_STATUS_PAYLOAD(false));
			//Do we need to send MOTD? I didn't test not sending. 1.12.1 doesn't have it.
			//await context.PayloadSendService.SendMessage(new SMSG_MOTD_PAYLOAD(new string[1] { "Powered by Glader's FreecraftCore" }));
			await context.PayloadSendService.SendMessage(new SMSG_LEARNED_DANCE_MOVES_PAYLOAD());
			//await context.PayloadSendService.SendMessage(new SMSG_INSTANCE_DIFFICULTY_PAYLOAD()); //default difficulty
			//await context.PayloadSendService.SendMessage(new SMSG_CONTACT_LIST_PAYLOAD()); //send empty contact list

			//Forward the loginrequest and we will transform all the server payloads to the wotlk version as they come back in
			await context.ProxyConnection.SendMessage(payload);
		}
	}
}
