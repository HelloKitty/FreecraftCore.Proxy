using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Fasterflect;
using FreecraftCore.Packet.CharSelect;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// In vanilla there is a different structure for the char list.
	/// So we must adjust it to the wotlk version before we forward it otherwise
	/// we will encounter major issues. Client crashes or failed serialization.
	/// </summary>
	[ServerPayloadHandler]
	public sealed class GameCharacterListResponsePayloadHandler : BaseGameServerPayloadHandler<CharacterListResponse_Vanilla>
	{
		/// <inheritdoc />
		public GameCharacterListResponsePayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{

		}

		/// <inheritdoc />
		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, CharacterListResponse_Vanilla payload)
		{
			if(Logger.IsInfoEnabled)
				Logger.Info($"Captured CharListResponse_Vanilla packet. Transforming to wotlk version");

			//Transforms all the characters to their wotlk structure.
			CharacterScreenCharacter[] characters = payload.Characters
				.Select(TransformCharacterToWotlk)
				.ToArray();

			return context.ProxyConnection.SendMessage(new CharacterListResponse(characters));
		}

		/// <summary>
		/// TODO DOC
		/// </summary>
		/// <param name="vanillaItem"></param>
		/// <returns></returns>
		private CharacterScreenItem TransformItemToWotlk(CharacterScreenItem_Vanilla vanillaItem)
		{
			//TODO: Is 0 ok for enchant id?
			return new CharacterScreenItem(vanillaItem.DisplayId, vanillaItem.InventoryType, 0);
		}

		private CharacterScreenCharacter TransformCharacterToWotlk(CharacterScreenCharacter_Vanilla vanillaCharacter)
		{
			CharacterScreenItem[] wotlkCharacterItems = vanillaCharacter.VisualEquipmentItems
				.Select(TransformItemToWotlk)
				.ToArray();

			//TODO: Trinitycore doesn't send this bag data? Why did Jackpoz have this? What should we send?
			return new CharacterScreenCharacter(vanillaCharacter.Data, 0, vanillaCharacter.isFirstLogin, vanillaCharacter.PetInformation, wotlkCharacterItems, BuildCharacterBags());
		}

		private static CharacterScreenBag[] BuildCharacterBags()
		{
			//TODO: Once the ctor for the bags is not private anymore we should switch off activator.
			return Enumerable.Repeat(new CharacterScreenBag(0, 0, 0), 4)
				.ToArray();
		}
	}
}
