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
	/// The <see cref="SMSG_QUERY_TIME_RESPONSE_Payload_Vanilla"/> to <see cref="SMSG_QUERY_TIME_RESPONSE_Payload"/>
	/// converter handler.
	/// </summary>
	[ServerPayloadHandler]
	public sealed class WotlkToVanillaQueryTimeResponsePayloadHandler : GameServerPayloadConverterHandler<SMSG_QUERY_TIME_RESPONSE_Payload_Vanilla, SMSG_QUERY_TIME_RESPONSE_Payload>
	{
		/// <inheritdoc />
		public WotlkToVanillaQueryTimeResponsePayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{
		}

		/// <inheritdoc />
		protected override SMSG_QUERY_TIME_RESPONSE_Payload ConvertToOutputPayload(SMSG_QUERY_TIME_RESPONSE_Payload_Vanilla payload)
		{
			//TODO: Will this be ok? We don't really have daily times in 1.12.1 so unsure what to send.
			return new SMSG_QUERY_TIME_RESPONSE_Payload(payload.CurrentTime, payload.CurrentTime * 2);
		}
	}
}
