using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// Type converter from Vanilla <see cref="MovementBlockData_Vanilla"/> to Wotlk <see cref="MovementBlockData"/>.
	/// </summary>
	public sealed class VanillaToWotlkMovementBlockDataTypeConverter : ITypeConverterProvider<MovementBlockData_Vanilla, MovementBlockData>
	{
		private ITypeConverterProvider<ObjectUpdateFlags_Vanilla, ObjectUpdateFlags> ObjectUpdateFlagsConverter { get; }

		private ITypeConverterProvider<MovementInfo_Vanilla, MovementInfo> MoveInfoConverter { get; }
		
		private ITypeConverterProvider<SplineInfo_Vanilla, SplineInfo> SplineInfoConverter { get; }

		private ITypeConverterProvider<CorpseInfo_Vanilla, CorpseInfo> CorpseInfoConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkMovementBlockDataTypeConverter([NotNull] ITypeConverterProvider<ObjectUpdateFlags_Vanilla, ObjectUpdateFlags> objectUpdateFlagsConverter, [NotNull] ITypeConverterProvider<MovementInfo_Vanilla, MovementInfo> moveInfoConverter, [NotNull] ITypeConverterProvider<SplineInfo_Vanilla, SplineInfo> splineInfoConverter, [NotNull] ITypeConverterProvider<CorpseInfo_Vanilla, CorpseInfo> corpseInfoConverter)
		{
			ObjectUpdateFlagsConverter = objectUpdateFlagsConverter ?? throw new ArgumentNullException(nameof(objectUpdateFlagsConverter));
			MoveInfoConverter = moveInfoConverter ?? throw new ArgumentNullException(nameof(moveInfoConverter));
			SplineInfoConverter = splineInfoConverter ?? throw new ArgumentNullException(nameof(splineInfoConverter));
			CorpseInfoConverter = corpseInfoConverter ?? throw new ArgumentNullException(nameof(corpseInfoConverter));
		}

		/// <inheritdoc />
		public MovementBlockData Convert(MovementBlockData_Vanilla fromObj)
		{
			if(fromObj == null) return null;

			ObjectUpdateFlags flags = ObjectUpdateFlagsConverter.Convert(fromObj.UpdateFlags);

			//Should be null if the object isn't living.
			MovementInfo info = MoveInfoConverter.Convert(fromObj.MoveInfo);

			SplineInfo spline = SplineInfoConverter.Convert(fromObj.SplineInformation);
			CorpseInfo corpseInfo = CorpseInfoConverter.Convert(fromObj.DeadMovementInformation);

			MovementBlockData data = new MovementBlockData(flags, info, fromObj.MovementSpeeds?.Concat(Enumerable.Repeat(1.0f, 3))?.ToArray(), spline, corpseInfo, null,
				/*TC always sends 0 but we can try this*/ (int)fromObj.HighGuid, /*Mangos always sends for UPDATE_ALL 0x10 AKA LOW_GUID. We can try TC hack if this doesn't work*/ 0x1,
				/*This is target*/ flags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_HAS_TARGET) ? fromObj.FullGuid : PackedGuid.Empty, fromObj.TransportTime, null, /*This is a packed QUATERRION but Vanilla doesn't send anything. TODO compute this*/ 0);

			return data;
		}
	}
}
