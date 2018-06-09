using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore
{
	/// <summary>
	/// Type converter for <see cref="MovementFlags_Vanilla"/> and <see cref="MovementFlag"/>.
	/// </summary>
	public sealed class MovementFlagsTypeConverter : ITypeConverterProvider<MovementFlags_Vanilla, MovementFlag>,
		ITypeConverterProvider<MovementFlag, MovementFlags_Vanilla>
	{
		public MovementFlagsTypeConverter()
		{
			
		}

		//This is not inlineable unless they reference the class method directly
		//but one day coreclr may implement partial virtualcall inlining so we will
		//leave these here anyway.
		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MovementFlag Convert(MovementFlags_Vanilla moveInfoMovementFlags)
		{
			//First 11 bits mean the same thing
			MovementFlag wotlkMoveFlags = (MovementFlag)((int)moveInfoMovementFlags);
			//First 11 bits mean the same thing
			wotlkMoveFlags = (MovementFlag)((int)moveInfoMovementFlags & 0b0111_1111_1111);

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_ROOT))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_ROOT;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_FLYING))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_FLYING;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_FALLING))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_FALLING;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_FALLINGFAR))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_FALLING_FAR;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_SWIMMING))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_SWIMMING;

			//This means we need to provide spline information in the move info
			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_SPLINE_ENABLED))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_SPLINE_ENABLED;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_CAN_FLY))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_CAN_FLY;

			//Skip flying old
			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_ONTRANSPORT))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_ONTRANSPORT;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_SPLINE_ELEVATION))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_SPLINE_ELEVATION;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_WATERWALKING))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_WATERWALKING;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_SAFE_FALL))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_FALLING_SLOW;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_HOVER))
				wotlkMoveFlags |= MovementFlag.MOVEMENTFLAG_HOVER;

			return wotlkMoveFlags;
		}

		//This is not inlineable unless they reference the class method directly
		//but one day coreclr may implement partial virtualcall inlining so we will
		//leave these here anyway.
		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MovementFlags_Vanilla Convert(MovementFlag moveInfoMovementFlags)
		{
			//First 11 bits mean the same thing
			MovementFlags_Vanilla newFlags = (MovementFlags_Vanilla)((int)moveInfoMovementFlags);
			//First 11 bits mean the same thing
			newFlags = (MovementFlags_Vanilla)((int)moveInfoMovementFlags & 0b0111_1111_1111);

			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_ROOT))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_ROOT;

			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_FLYING))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_FLYING;

			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_FALLING))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_FALLING;

			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_FALLING_FAR))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_FALLINGFAR;

			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_SWIMMING))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_SWIMMING;

			//This means we need to provide spline information in the move info
			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_SPLINE_ENABLED))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_SPLINE_ENABLED;

			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_CAN_FLY))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_CAN_FLY;

			//Skip flying old
			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_ONTRANSPORT))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_ONTRANSPORT;

			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_SPLINE_ELEVATION))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_SPLINE_ELEVATION;

			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_WATERWALKING))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_WATERWALKING;

			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_FALLING_SLOW))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_SAFE_FALL;

			if(moveInfoMovementFlags.HasFlag(MovementFlag.MOVEMENTFLAG_HOVER))
				newFlags |= MovementFlags_Vanilla.MOVEFLAG_HOVER;

			return newFlags;
		}
	}
}
