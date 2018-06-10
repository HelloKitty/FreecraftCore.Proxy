using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore
{
	public sealed class ItemQueryResponseInfoTypeConverter : ITypeConverterProvider<ItemQueryResponseInfo_Vanilla, ItemQueryResponseInfo>
	{
		public static ItemSocketInfo StaticItemSocketCachedValue = new ItemSocketInfo(Enumerable.Repeat(new SocketEntry(SocketColor.SOCKET_COLOR_BLUE, 0), 3).ToArray(), 0, 0); 

		/// <inheritdoc />
		public ItemQueryResponseInfo Convert(ItemQueryResponseInfo_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			//TODO: When ctor is fixed return to this
			//TODO: Incompatibility from vanilla to wotlk for item damage mods
			return new ItemQueryResponseInfo(fromObject.ClassType, fromObject.SubClassType, 0, fromObject.ItemNames, fromObject.DisplayId,
				fromObject.Quality, fromObject.ItemFlags, (ItemFlags2)0, fromObject.BuyPrice, fromObject.SellPrice, fromObject.InventoryType, fromObject.AllowableClass,
				fromObject.AllowableRace, fromObject.ItemLevel, fromObject.RequiredLevel, fromObject.RequiredSkill, fromObject.RequiredSkillRank, fromObject.RequiredSpell, fromObject.RequiredHonorRank,
				fromObject.RequiredCityRank, fromObject.RequiredReptuationFaction, fromObject.RequiredReptuationRank, fromObject.MaxCount, fromObject.MaxStackable, fromObject.ContainerSlots, fromObject.StatInfos, 0, 0, 
				fromObject.ItemDamageMods.Take(2).ToArray(), fromObject.Resistances, fromObject.Delay, fromObject.AmmoType, fromObject.RangedModRange, fromObject.SpellInfos, fromObject.BondingType,
				fromObject.ItemDescription, fromObject.PageText, fromObject.LanguageId, fromObject.PageMaterial, fromObject.StartQuest, fromObject.LockID, fromObject.PageMaterial,
				fromObject.Sheath, fromObject.RandomProperty, 0, fromObject.Block, fromObject.ItemSet, fromObject.Maxdurability, fromObject.Area, fromObject.Map, fromObject.BagFamily, StaticItemSocketCachedValue, 0, 0, 0, 0, 0);
		}
	}
}
