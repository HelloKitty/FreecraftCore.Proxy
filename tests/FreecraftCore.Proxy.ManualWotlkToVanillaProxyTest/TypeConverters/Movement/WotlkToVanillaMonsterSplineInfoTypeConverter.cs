using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// Type converter for the Wotlk <see cref="MonsterSplineInfo"/> to the Vanilla <see cref="MonsterSplineInfo_Vanilla"/>.
	/// </summary>
	public sealed class WotlkToVanillaMonsterSplineInfoTypeConverter : ITypeConverterProvider<MonsterSplineInfo, MonsterSplineInfo_Vanilla>
	{
		/// <summary>
		/// The type convert for the spline flags to be used in the monster
		/// spline conversion.
		/// </summary>
		private ITypeConverterProvider<SplineMoveFlags, SplineMoveFlags_Vanilla> SplineFlagsConverter { get; }

		/// <inheritdoc />
		public WotlkToVanillaMonsterSplineInfoTypeConverter([NotNull] ITypeConverterProvider<SplineMoveFlags, SplineMoveFlags_Vanilla> splineFlagsConverter)
		{
			SplineFlagsConverter = splineFlagsConverter ?? throw new ArgumentNullException(nameof(splineFlagsConverter));
		}

		//This is not inlineable unless they reference the class method directly
		//but one day coreclr may implement partial virtualcall inlining so we will
		//leave these here anyway.
		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MonsterSplineInfo_Vanilla Convert(MonsterSplineInfo fromObj)
		{
			if(fromObj == null) return null;

			return new MonsterSplineInfo_Vanilla(SplineFlagsConverter.Convert(fromObj.SplineFlags), fromObj.SplineDuration,
				fromObj.OptionalCatMulRomSplinePoints, fromObj.OptionalLinearPathInformation);
		}
	}
}
