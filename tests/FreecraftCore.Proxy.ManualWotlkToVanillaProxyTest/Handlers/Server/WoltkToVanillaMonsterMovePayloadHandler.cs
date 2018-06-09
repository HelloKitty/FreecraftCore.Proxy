using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class WoltkToVanillaMonsterMovePayloadHandler : GameServerPayloadConverterHandler<SMSG_MONSTER_MOVE_Payload_Vanilla, SMSG_MONSTER_MOVE_Payload>
	{
		private ITypeConverterProvider<MonsterSplineInfo_Vanilla, MonsterSplineInfo> MonsterSplineConverter { get; }

		/// <inheritdoc />
		public WoltkToVanillaMonsterMovePayloadHandler([NotNull] ILog logger, [NotNull] ITypeConverterProvider<MonsterSplineInfo_Vanilla, MonsterSplineInfo> monsterSplineConverter) 
			: base(logger)
		{
			MonsterSplineConverter = monsterSplineConverter ?? throw new ArgumentNullException(nameof(monsterSplineConverter));
		}

		/// <inheritdoc />
		protected override SMSG_MONSTER_MOVE_Payload ConvertToOutputPayload(SMSG_MONSTER_MOVE_Payload_Vanilla payload)
		{
			//TC sends 0 for the unk
			return new SMSG_MONSTER_MOVE_Payload(payload.MonsterGuid, 0, payload.InitialMovePoint, 
				payload.SplineId, payload.MoveInfo, MonsterSplineConverter.Convert(payload.OptionalSplineInformation));
		}
	}
}
