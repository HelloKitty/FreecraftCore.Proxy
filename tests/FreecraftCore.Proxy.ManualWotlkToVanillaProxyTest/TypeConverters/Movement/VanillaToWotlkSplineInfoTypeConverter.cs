using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class VanillaToWotlkSplineInfoTypeConverter : ITypeConverterProvider<SplineInfo_Vanilla, SplineInfo>
	{
		private ITypeConverterProvider<SplineMoveFlags_Vanilla, SplineMoveFlags> SplineFlagsConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkSplineInfoTypeConverter([NotNull] ITypeConverterProvider<SplineMoveFlags_Vanilla, SplineMoveFlags> splineFlagsConverter)
		{
			SplineFlagsConverter = splineFlagsConverter ?? throw new ArgumentNullException(nameof(splineFlagsConverter));
		}

		/// <inheritdoc />
		public SplineInfo Convert(SplineInfo_Vanilla fromObj)
		{
			if(fromObj == null) return null;

			//TODO: Check these default wotlk values
			return new SplineInfo(SplineFlagsConverter.Convert(fromObj.SplineFlags), fromObj.FinalTarget, fromObj.FinalOrientation,
				fromObj.FinalPoint, fromObj.SplineTime, fromObj.SplineFullTime, fromObj.SplineId, 1.0f, 1.0f,
				0.0f, 0, fromObj.WayPoints, SplineMode.Linear, fromObj.SplineEndpoint);
		}
	}
}
