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
	/// Type converter for Vanilla <see cref="MovementInfo_Vanilla"/> to <see cref="MovementInfo"/>.
	/// </summary>
	public sealed class VanillaToWotlkMovementInfoTypeConverter : ITypeConverterProvider<MovementInfo_Vanilla, MovementInfo>
	{
		/// <summary>
		/// The move flags converter for the move info conversion.
		/// </summary>
		public ITypeConverterProvider<MovementFlags_Vanilla, MovementFlag> MoveFlagsConverter { get; }

		/// <summary>
		/// The transportinfo converter for the move info conversion.
		/// </summary>
		public ITypeConverterProvider<TransportationInfo_Vanilla, TransportationInfo> TransportInfoConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkMovementInfoTypeConverter([NotNull] ITypeConverterProvider<MovementFlags_Vanilla, MovementFlag> moveFlagsConverter, [NotNull] ITypeConverterProvider<TransportationInfo_Vanilla, TransportationInfo> transportInfoConverter)
		{
			MoveFlagsConverter = moveFlagsConverter ?? throw new ArgumentNullException(nameof(moveFlagsConverter));
			TransportInfoConverter = transportInfoConverter ?? throw new ArgumentNullException(nameof(transportInfoConverter));
		}

		//This is not inlineable unless they reference the class method directly
		//but one day coreclr may implement partial virtualcall inlining so we will
		//leave these here anyway.
		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MovementInfo Convert(MovementInfo_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			MovementFlag moveFlags = MoveFlagsConverter.Convert(fromObject.MoveFlags);

			MovementInfo info = new MovementInfo(moveFlags, MovementFlagExtra.None,
				fromObject.TimeStamp, fromObject.Position,
				fromObject.Orientation, TransportInfoConverter.Convert(fromObject.TransportationInformation),
				fromObject.TransportTime, fromObject.MovePitch, fromObject.FallTime, fromObject.FallData, fromObject.SplineElevation);

			return info;
		}
	}
}
