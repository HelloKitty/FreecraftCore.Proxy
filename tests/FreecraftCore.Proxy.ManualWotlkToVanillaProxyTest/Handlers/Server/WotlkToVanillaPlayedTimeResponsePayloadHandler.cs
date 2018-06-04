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
	/// The <see cref="SMSG_PLAYED_TIME_Payload_Vanilla"/> to <see cref="SMSG_PLAYED_TIME_Payload"/>
	/// converter handler.
	/// </summary>
	[ServerPayloadHandler]
	public sealed class WotlkToVanillaPlayedTimeResponsePayloadHandler : GameServerPayloadConverterHandler<SMSG_PLAYED_TIME_Payload_Vanilla, SMSG_PLAYED_TIME_Payload>
	{
		/// <inheritdoc />
		public WotlkToVanillaPlayedTimeResponsePayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{
		}

		/// <inheritdoc />
		protected override SMSG_PLAYED_TIME_Payload ConvertToOutputPayload(SMSG_PLAYED_TIME_Payload_Vanilla payload)
		{
			//TODO: Will this be ok? We don't really have daily times in 1.12.1 so unsure what to send.
			return new SMSG_PLAYED_TIME_Payload(payload.TotalPlayTime, payload.TotalPlayTime * 2);
		}
	}
}
