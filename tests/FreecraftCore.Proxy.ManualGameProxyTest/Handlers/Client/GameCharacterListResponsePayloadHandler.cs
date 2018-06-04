using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Fasterflect;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	[ServerPayloadHandler]
	public sealed class GameCharacterListResponsePayloadHandler : BaseGameServerPayloadHandler<CharacterListResponse>
	{
		/// <inheritdoc />
		public GameCharacterListResponsePayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{

		}

		/// <inheritdoc />
		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, CharacterListResponse payload)
		{
			if(Logger.IsInfoEnabled)
				Logger.Info($"Captured CharListResponse packet. ProxyType: {context.ProxyConnection.GetType()} Going to Forward");

			//CharacterScreenCharacter character = payload.Characters.First();

			//character.SetFieldValue($"<{nameof(CharacterScreenCharacter.Race)}>k__BackingField", (CharacterRace)12, Flags.AllMembers);

			return context.ProxyConnection.SendMessageImmediately(payload, DeliveryMethod.ReliableOrdered);
		}
	}
}
