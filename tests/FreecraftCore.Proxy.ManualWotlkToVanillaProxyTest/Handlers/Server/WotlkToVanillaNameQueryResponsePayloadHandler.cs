using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Fasterflect;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// The <see cref="SMSG_NAME_QUERY_RESPONSE_Payload_Vanilla"/> to <see cref="SMSG_NAME_QUERY_RESPONSE_Payload"/>
	/// converter handler.
	/// </summary>
	[ServerPayloadHandler]
	public sealed class WotlkToVanillaNameQueryResponsePayloadHandler : GameServerPayloadConverterHandler<SMSG_NAME_QUERY_RESPONSE_Payload_Vanilla, SMSG_NAME_QUERY_RESPONSE_Payload>
	{
		/// <inheritdoc />
		public WotlkToVanillaNameQueryResponsePayloadHandler([NotNull] ILog logger) : base(logger)
		{
		}

		/// <inheritdoc />
		protected override SMSG_NAME_QUERY_RESPONSE_Payload ConvertToOutputPayload(SMSG_NAME_QUERY_RESPONSE_Payload_Vanilla payload)
		{
			//TODO: Look into how client handles unresponded to name queries in 3.3.5. Could be a leak.
			//Vanilla will always send a response, so just send back as success. Which could be problematic since the 3.3.5 expects failure responses maybe?
			return new SMSG_NAME_QUERY_RESPONSE_Payload(new PackedGuid(payload.RequestedGuid), payload.Result);
		}
	}
}
