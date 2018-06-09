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
		public ITypeConverterProvider<CharacterScreenCharacter_Vanilla, CharacterScreenCharacter> CharacterConverter { get; }

		/// <inheritdoc />
		public GameCharacterListResponsePayloadHandler([NotNull] ILog logger, [NotNull] ITypeConverterProvider<CharacterScreenCharacter_Vanilla, CharacterScreenCharacter> characterConverter) 
			: base(logger)
		{
			CharacterConverter = characterConverter ?? throw new ArgumentNullException(nameof(characterConverter));
		}

		/// <inheritdoc />
		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, CharacterListResponse_Vanilla payload)
		{
			if(Logger.IsInfoEnabled)
				Logger.Info($"Captured CharListResponse_Vanilla packet. Transforming to wotlk version");

			//Transforms all the characters to their wotlk structure.
			CharacterScreenCharacter[] characters = payload.Characters
				.Select(CharacterConverter.Convert)
				.ToArray();

			return context.ProxyConnection.SendMessage(new CharacterListResponse(characters));
		}	
	}
}
