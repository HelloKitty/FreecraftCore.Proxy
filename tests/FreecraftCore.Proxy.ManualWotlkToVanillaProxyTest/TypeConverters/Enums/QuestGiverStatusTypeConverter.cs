using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore
{
	public sealed class QuestGiverStatusTypeConverter : ITypeConverterProvider<QuestGiverStatus_Vanilla, QuestGiverStatus>
	{
		/// <inheritdoc />
		public QuestGiverStatus Convert(QuestGiverStatus_Vanilla fromObject)
		{
			switch(fromObject)
			{
				case QuestGiverStatus_Vanilla.DIALOG_STATUS_NONE:
					return QuestGiverStatus.DIALOG_STATUS_NONE;
				case QuestGiverStatus_Vanilla.DIALOG_STATUS_UNAVAILABLE:
					return QuestGiverStatus.DIALOG_STATUS_UNAVAILABLE;
				//TODO: Is this the best match?
				case QuestGiverStatus_Vanilla.DIALOG_STATUS_CHAT:
					return QuestGiverStatus.DIALOG_STATUS_AVAILABLE;
				case QuestGiverStatus_Vanilla.DIALOG_STATUS_INCOMPLETE:
					return QuestGiverStatus.DIALOG_STATUS_INCOMPLETE;
				case QuestGiverStatus_Vanilla.DIALOG_STATUS_REWARD_REP:
					return QuestGiverStatus.DIALOG_STATUS_LOW_LEVEL_REWARD_REP;
				case QuestGiverStatus_Vanilla.DIALOG_STATUS_AVAILABLE:
					return QuestGiverStatus.DIALOG_STATUS_AVAILABLE;
				//TODO: Is this ok?
				case QuestGiverStatus_Vanilla.DIALOG_STATUS_REWARD_OLD:
					return QuestGiverStatus.DIALOG_STATUS_REWARD;
				case QuestGiverStatus_Vanilla.DIALOG_STATUS_REWARD2:
					return QuestGiverStatus.DIALOG_STATUS_REWARD2;
				case QuestGiverStatus_Vanilla.DIALOG_STATUS_UNDEFINED:
					return QuestGiverStatus.DIALOG_STATUS_SCRIPTED_NO_STATUS;

				default:
					throw new InvalidOperationException($"Encountered unknown {nameof(QuestGiverStatus_Vanilla)} value: {fromObject}");
			}
		}
	}
}