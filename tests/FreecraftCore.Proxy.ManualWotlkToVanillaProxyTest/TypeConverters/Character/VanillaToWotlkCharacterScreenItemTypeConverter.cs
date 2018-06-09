using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore
{
	public sealed class VanillaToWotlkCharacterScreenItemTypeConverter : ITypeConverterProvider<CharacterScreenItem_Vanilla, CharacterScreenItem>
	{
		/// <inheritdoc />
		public CharacterScreenItem Convert(CharacterScreenItem_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			//TODO: Is 0 ok for enchant id?
			return new CharacterScreenItem(fromObject.DisplayId, fromObject.InventoryType, 0);
		}
	}
}
