using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/*public sealed class WotlkToVanillaItemQueryResponsePayloadHandler : GameServerPayloadConverterHandler<SMSG_ITEM_QUERY_SINGLE_RESPONSE_Payload_Vanilla, SMSG_ITEM_QUERY_SINGLE_RESPONSE_Payload>
	{
		private ITypeConverterProvider<ItemQueryResponseInfo_Vanilla, ItemQueryResponseInfo> ItemQueryResponseConverter { get; }

		/// <inheritdoc />
		public WotlkToVanillaItemQueryResponsePayloadHandler([NotNull] ILog logger, [NotNull] ITypeConverterProvider<ItemQueryResponseInfo_Vanilla, ItemQueryResponseInfo> itemQueryResponseConverter) 
			: base(logger)
		{
			ItemQueryResponseConverter = itemQueryResponseConverter ?? throw new ArgumentNullException(nameof(itemQueryResponseConverter));
		}

		/// <inheritdoc />
		protected override SMSG_ITEM_QUERY_SINGLE_RESPONSE_Payload ConvertToOutputPayload(SMSG_ITEM_QUERY_SINGLE_RESPONSE_Payload_Vanilla payload)
		{
			if(payload.IsSuccessful)
				return new SMSG_ITEM_QUERY_SINGLE_RESPONSE_Payload(payload.QueryId, ItemQueryResponseConverter.Convert(payload.Result));
			
			//Failed ones don't need conversion.
			return new SMSG_ITEM_QUERY_SINGLE_RESPONSE_Payload((uint)payload.QueryId);
		}
	}*/
}
