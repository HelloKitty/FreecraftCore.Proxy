using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaQuestGiveStatusResponsePayloadHandler : GameServerPayloadConverterHandler<SMSG_QUESTGIVER_STATUS_Payload_Vanilla, SMSG_QUESTGIVER_STATUS_Payload>
	{
		private ITypeConverterProvider<QuestGiverStatus_Vanilla, QuestGiverStatus> QuestGiverStatusConverter { get; }

		/// <inheritdoc />
		public WotlkToVanillaQuestGiveStatusResponsePayloadHandler([NotNull] ILog logger, [NotNull] ITypeConverterProvider<QuestGiverStatus_Vanilla, QuestGiverStatus> questGiverStatusConverter) 
			: base(logger)
		{
			QuestGiverStatusConverter = questGiverStatusConverter ?? throw new ArgumentNullException(nameof(questGiverStatusConverter));
		}

		/// <inheritdoc />
		protected override SMSG_QUESTGIVER_STATUS_Payload ConvertToOutputPayload(SMSG_QUESTGIVER_STATUS_Payload_Vanilla payload)
		{
			return new SMSG_QUESTGIVER_STATUS_Payload(payload.QueryId, QuestGiverStatusConverter.Convert(payload.Status));
		}
	}
}
