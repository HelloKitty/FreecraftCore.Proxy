using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaInitialSpellsPayloadHandler : GameServerPayloadConverterHandler<SMSG_INITIAL_SPELLS_Payload_Vanilla, SMSG_INITIAL_SPELLS_Payload>
	{
		/// <inheritdoc />
		public WotlkToVanillaInitialSpellsPayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{
		}

		/// <inheritdoc />
		protected override SMSG_INITIAL_SPELLS_Payload ConvertToOutputPayload(SMSG_INITIAL_SPELLS_Payload_Vanilla payload)
		{
			InitialSpellData<int>[] initialSpellDatas = payload.Data.SpellIds
				.Select(s => new InitialSpellData<int>(s.SpellId, s.UnkShort))
				.ToArray();

			InitialSpellCooldown<int>[] cooldowns = payload.Data.SpellCooldowns?
				.Select(c => new InitialSpellCooldown<int>(c.SpellId, c.ItemId, c.CategoryId, c.SpellCooldown, c.CategoryCooldown))
				.ToArray();

			return new SMSG_INITIAL_SPELLS_Payload(new InitialSpellDataBlock<int>(initialSpellDatas, cooldowns));
		}
	}
}
