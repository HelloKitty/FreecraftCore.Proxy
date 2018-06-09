using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore
{
	/// <summary>
	/// Type converter for <see cref="SplineMoveFlags_Vanilla"/> and <see cref="SplineMoveFlags"/>.
	/// </summary>
	public sealed class SplineMoveFlagsTypeConverter : ITypeConverterProvider<SplineMoveFlags, SplineMoveFlags_Vanilla>,
		ITypeConverterProvider<SplineMoveFlags_Vanilla, SplineMoveFlags>
	{
		public SplineMoveFlagsTypeConverter()
		{
			
		}

		//This is not inlineable unless they reference the class method directly
		//but one day coreclr may implement partial virtualcall inlining so we will
		//leave these here anyway.
		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SplineMoveFlags_Vanilla Convert(SplineMoveFlags splineFlags)
		{
			SplineMoveFlags_Vanilla flags = 0;

			if((splineFlags & SplineMoveFlags.Final_Angle) != 0)
				flags |= SplineMoveFlags_Vanilla.Final_Angle;

			if((splineFlags & SplineMoveFlags.Final_Target) != 0)
				flags |= SplineMoveFlags_Vanilla.Final_Target;

			if((splineFlags & SplineMoveFlags.Final_Point) != 0)
				flags |= SplineMoveFlags_Vanilla.Final_Point;

			if((splineFlags & SplineMoveFlags.Done) != 0)
				flags |= SplineMoveFlags_Vanilla.Done;

			if((splineFlags & SplineMoveFlags.Cyclic) != 0)
				flags |= SplineMoveFlags_Vanilla.Cyclic;

			if((splineFlags & SplineMoveFlags.No_Spline) != 0)
				flags |= SplineMoveFlags_Vanilla.No_Spline;

			return flags;
		}

		//This is not inlineable unless they reference the class method directly
		//but one day coreclr may implement partial virtualcall inlining so we will
		//leave these here anyway.
		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SplineMoveFlags Convert(SplineMoveFlags_Vanilla splineFlags)
		{
			SplineMoveFlags flags = 0;

			if(splineFlags.HasFlag(SplineMoveFlags_Vanilla.Final_Angle))
				flags |= SplineMoveFlags.Final_Angle;

			if(splineFlags.HasFlag(SplineMoveFlags_Vanilla.Final_Target))
				flags |= SplineMoveFlags.Final_Target;

			if(splineFlags.HasFlag(SplineMoveFlags_Vanilla.Final_Point))
				flags |= SplineMoveFlags.Final_Point;

			if(splineFlags.HasFlag(SplineMoveFlags_Vanilla.Done))
				flags |= SplineMoveFlags.Done;

			if(splineFlags.HasFlag(SplineMoveFlags_Vanilla.Cyclic))
				flags |= SplineMoveFlags.Cyclic;

			if(splineFlags.HasFlag(SplineMoveFlags_Vanilla.No_Spline))
				flags |= SplineMoveFlags.No_Spline;

			return flags;
		}
	}
}
