using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Fasterflect;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/*[ServerPayloadHandler]
	public sealed class GameCharacterListResponsePayloadHandler : BaseGameServerPayloadHandler<CharacterListResponse>
	{
		/// <inheritdoc />
		public GameCharacterListResponsePayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{
		}

		/// <inheritdoc />
		public async override Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, CharacterListResponse payload)
		{
			//CharacterScreenCharacter character = payload.Characters.First();

			//character.SetFieldValue($"<{nameof(CharacterScreenCharacter.Race)}>k__BackingField", CharacterRace.Orc, Flags.AllMembers);

			await context.PayloadSendService.SendMessage(payload);
		}
	}*/
}
