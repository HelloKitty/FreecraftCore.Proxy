using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binarysharp.MemoryManagement;
using Common.Logging;
using GladNet;
using JetBrains.Annotations;
using Reinterpret.Net;

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

			await context.PayloadSendService.SendMessage(new SMSG_INSTANCE_DIFFICULTY_PAYLOAD());
			await context.PayloadSendService.SendMessage(new SMSG_FEATURE_SYSTEM_STATUS_PAYLOAD(false));
			//Do we need to send MOTD? I didn't test not sending. 1.12.1 doesn't have it.
			await context.PayloadSendService.SendMessage(new SMSG_MOTD_PAYLOAD(new string[1] {"Powered by Glader's FreecraftCore"}));
			await context.PayloadSendService.SendMessage(new SMSG_LEARNED_DANCE_MOVES_PAYLOAD());
			await context.PayloadSendService.SendMessage(new SMSG_INSTANCE_DIFFICULTY_PAYLOAD()); //default difficulty

			//TODO: Renable maybe when we renable this packet
			//await context.PayloadSendService.SendMessage(new SMSG_CONTACT_LIST_PAYLOAD()); //send empty contact list

			//Forward the loginrequest and we will transform all the server payloads to the wotlk version as they come back in
			await context.ProxyConnection.SendMessage(payload);

			//TODO: Implement this better
			//We also need to send a special spell in 3.3.5 called LoginSpell
			//It is sent by the 3.3.5 server to confirm the login.
			await Task.Delay(5000)
				.ConfigureAwait(true);
			/*ServerToClient: SMSG_SPELL_GO (0x0132) Length: 36 ConnIdx: 0 EP: 127.0.0.1:59538 Time: 06/07/2018 21:59:58.785 Number: 55
			Caster GUID: Full: 0x00000004 Type: Player Low: 4 Name: Test
			Caster Unit GUID: Full: 0x00000004 Type: Player Low: 4 Name: Test
			Cast Count: 0
			Spell ID: 836 (836)
			Cast Flags: 264449 (PendingCast, Unknown7, PredictedPower, Unknown16)
			Time: 48644
			Hit Count: 1
			[0] Hit GUID: Full: 0x00000004 Type: Player Low: 4 Name: Test
			Miss Count: 0
			Target Flags: 2 (Unit)
			Target GUID: 0x0
			Rune Cooldown: 4253*/



			Task.Factory.StartNew(async () =>
				{
					int i = 0;
					while(true)
					{
						await context.PayloadSendService.SendMessage(new SMSG_TIME_SYNC_REQ_DTO_PROXY() { Data = i.Reinterpret() });
						await Task.Delay(10000);
						i++;
					}
				})
				.ConfigureAwait(false);

			//TODO: Time is off here, will it cause issues?
			//await context.PayloadSendService.SendMessage(new SMSG_SPELL_GO_Payload(new PackedGuid(payload.CharacterGuid.RawGuidValue), new PackedGuid(payload.CharacterGuid.RawGuidValue), 0,
			//	836, (SpellCastFlag)264449, 0, new ObjectGuid[1] {payload.CharacterGuid}, new SpellMissInfo[0], BuildLoginSpellTargetInfo(), 0, null, null, 0));
		}

		private static SpellTargetInfo BuildLoginSpellTargetInfo()
		{
			return new SpellTargetInfo(SpellCastTargetFlag.TARGET_FLAG_UNIT, PackedGuid.Empty, null, null, null, null, null, null);
		}
	}
}
