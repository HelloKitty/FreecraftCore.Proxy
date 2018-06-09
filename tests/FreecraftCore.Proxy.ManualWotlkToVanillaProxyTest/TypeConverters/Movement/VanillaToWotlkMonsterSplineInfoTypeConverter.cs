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
	/// Type converter for the Vanilla <see cref="MonsterSplineInfo_Vanilla"/> to the Wotlk <see cref="MonsterSplineInfo"/>.
	/// </summary>
	public sealed class VanillaToWotlkMonsterSplineInfoTypeConverter : ITypeConverterProvider<MonsterSplineInfo_Vanilla, MonsterSplineInfo>
	{
		/// <summary>
		/// The type convert for the spline flags to be used in the monster
		/// spline conversion.
		/// </summary>
		private ITypeConverterProvider<SplineMoveFlags_Vanilla, SplineMoveFlags> SplineFlagsConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkMonsterSplineInfoTypeConverter([NotNull] ITypeConverterProvider<SplineMoveFlags_Vanilla, SplineMoveFlags> splineFlagsConverter)
		{
			SplineFlagsConverter = splineFlagsConverter ?? throw new ArgumentNullException(nameof(splineFlagsConverter));
		}

		//This is not inlineable unless they reference the class method directly
		//but one day coreclr may implement partial virtualcall inlining so we will
		//leave these here anyway.
		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MonsterSplineInfo Convert(MonsterSplineInfo_Vanilla fromObj)
		{
			if(fromObj == null) return null;

			return new MonsterSplineInfo(SplineFlagsConverter.Convert(fromObj.SplineFlags), null, fromObj.SplineDuration, null,
				fromObj.OptionalCatMulRomSplinePoints, fromObj.OptionalLinearPathInformation);
		}
	}
}
