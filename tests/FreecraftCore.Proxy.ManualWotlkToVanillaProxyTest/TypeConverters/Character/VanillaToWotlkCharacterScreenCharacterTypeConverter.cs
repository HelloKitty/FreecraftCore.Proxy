using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class VanillaToWotlkCharacterScreenCharacterTypeConverter : ITypeConverterProvider<CharacterScreenCharacter_Vanilla, CharacterScreenCharacter>
	{
		private ITypeConverterProvider<CharacterScreenItem_Vanilla, CharacterScreenItem> CharacterScreenItemConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkCharacterScreenCharacterTypeConverter([NotNull] ITypeConverterProvider<CharacterScreenItem_Vanilla, CharacterScreenItem> characterScreenItemConverter)
		{
			CharacterScreenItemConverter = characterScreenItemConverter ?? throw new ArgumentNullException(nameof(characterScreenItemConverter));
		}

		/// <inheritdoc />
		public CharacterScreenCharacter Convert(CharacterScreenCharacter_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			CharacterScreenItem[] wotlkCharacterItems = fromObject.VisualEquipmentItems
				.Select(CharacterScreenItemConverter.Convert)
				.ToArray();

			//TODO: Trinitycore doesn't send this bag data? Why did Jackpoz have this? What should we send?
			return new CharacterScreenCharacter(fromObject.Data, 0, fromObject.isFirstLogin, fromObject.PetInformation, wotlkCharacterItems, BuildCharacterBags());
		}

		private static CharacterScreenBag[] BuildCharacterBags()
		{
			//TODO: Once the ctor for the bags is not private anymore we should switch off activator.
			return Enumerable.Repeat(new CharacterScreenBag(0, 0, 0), 4)
				.ToArray();
		}
	}
}
