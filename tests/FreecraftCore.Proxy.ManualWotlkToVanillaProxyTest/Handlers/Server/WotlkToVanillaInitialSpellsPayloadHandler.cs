using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;
using Reinterpret.Net;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaInitialSpellsPayloadHandler : BaseGameServerPayloadHandler<SMSG_INITIAL_SPELLS_Payload_Vanilla>
	{
		/// <inheritdoc />
		public WotlkToVanillaInitialSpellsPayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{
		}

		/// <inheritdoc />
		private SMSG_INITIAL_SPELLS_Payload ConvertToOutputPayload(SMSG_INITIAL_SPELLS_Payload_Vanilla payload)
		{
			InitialSpellData<int>[] initialSpellDatas = payload.Data.SpellIds
				.Select(s => new InitialSpellData<int>(s.SpellId, s.UnkShort))
				.ToArray();

			InitialSpellCooldown<int>[] cooldowns = payload.Data.SpellCooldowns?
				.Select(c => new InitialSpellCooldown<int>(c.SpellId, c.ItemId, c.CategoryId, c.SpellCooldown, c.CategoryCooldown))
				.ToArray();

			return new SMSG_INITIAL_SPELLS_Payload(new InitialSpellDataBlock<int>(initialSpellDatas, cooldowns));
		}

		/// <inheritdoc />
		public override async Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SMSG_INITIAL_SPELLS_Payload_Vanilla payload)
		{
			SMSG_INITIAL_SPELLS_Payload outputPayload = ConvertToOutputPayload(payload);

			await context.ProxyConnection.SendMessage(outputPayload);

			//TC sends this after the initial spells
			await context.ProxyConnection.SendMessage(new SMSG_SEND_UNLEARN_SPELLS_DTO_PROXY() { Data = 0.Reinterpret() });
		}
	}
}
