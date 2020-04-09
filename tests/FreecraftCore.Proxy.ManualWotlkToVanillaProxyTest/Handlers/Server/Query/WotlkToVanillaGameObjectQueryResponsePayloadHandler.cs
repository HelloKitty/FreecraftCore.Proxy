using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/*public sealed class WotlkToVanillaGameObjectQueryResponsePayloadHandler : GameServerPayloadConverterHandler<SMSG_GAMEOBJECT_QUERY_RESPONSE_Payload_Vanilla, SMSG_GAMEOBJECT_QUERY_RESPONSE_Payload>
	{
		private ITypeConverterProvider<GameObjectQueryResponseInfo_Vanilla, GameObjectQueryResponseInfo> QueryResponseInfoConverter { get; }

		/// <inheritdoc />
		public WotlkToVanillaGameObjectQueryResponsePayloadHandler([NotNull] ILog logger, [NotNull] ITypeConverterProvider<GameObjectQueryResponseInfo_Vanilla, GameObjectQueryResponseInfo> queryResponseInfoConverter) 
			: base(logger)
		{
			QueryResponseInfoConverter = queryResponseInfoConverter ?? throw new ArgumentNullException(nameof(queryResponseInfoConverter));
		}

		/// <inheritdoc />
		protected override SMSG_GAMEOBJECT_QUERY_RESPONSE_Payload ConvertToOutputPayload(SMSG_GAMEOBJECT_QUERY_RESPONSE_Payload_Vanilla payload)
		{
			if(payload.IsSuccessful)
				return new SMSG_GAMEOBJECT_QUERY_RESPONSE_Payload(payload.QueryId, QueryResponseInfoConverter.Convert(payload.Result));

			return new SMSG_GAMEOBJECT_QUERY_RESPONSE_Payload((uint)payload.QueryId);
		}
	}*/
}
