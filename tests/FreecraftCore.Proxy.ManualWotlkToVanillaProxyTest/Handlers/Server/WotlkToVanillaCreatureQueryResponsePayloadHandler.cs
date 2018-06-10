using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaCreatureQueryResponsePayloadHandler : GameServerPayloadConverterHandler<SMSG_CREATURE_QUERY_RESPONSE_Payload_Vanilla, SMSG_CREATURE_QUERY_RESPONSE_Payload>
	{
		private ITypeConverterProvider<CreatureQueryResponseInfo_Vanilla, CreatureQueryResponseInfo> CreatureQueryResponseConverter { get; }

		/// <inheritdoc />
		public WotlkToVanillaCreatureQueryResponsePayloadHandler([NotNull] ILog logger, [NotNull] ITypeConverterProvider<CreatureQueryResponseInfo_Vanilla, CreatureQueryResponseInfo> creatureQueryResponseConverter) 
			: base(logger)
		{
			CreatureQueryResponseConverter = creatureQueryResponseConverter ?? throw new ArgumentNullException(nameof(creatureQueryResponseConverter));
		}

		/// <inheritdoc />
		protected override SMSG_CREATURE_QUERY_RESPONSE_Payload ConvertToOutputPayload(SMSG_CREATURE_QUERY_RESPONSE_Payload_Vanilla payload)
		{
			return new SMSG_CREATURE_QUERY_RESPONSE_Payload(payload.QueryId, CreatureQueryResponseConverter.Convert(payload.Result));
		}
	}
}
