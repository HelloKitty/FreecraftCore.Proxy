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
	/// Type converter for Wotlk <see cref="MovementInfo"/> to <see cref="MovementInfo_Vanilla"/>.
	/// </summary>
	public sealed class WotlkToVanillaMovementInfoTypeConverter : ITypeConverterProvider<MovementInfo, MovementInfo_Vanilla>
	{
		/// <summary>
		/// The move flags converter for the move info conversion.
		/// </summary>
		public ITypeConverterProvider<MovementFlag, MovementFlags_Vanilla> MoveFlagsConverter { get; }

		/// <summary>
		/// The transportinfo converter for the move info conversion.
		/// </summary>
		public ITypeConverterProvider<TransportationInfo, TransportationInfo_Vanilla> TransportInfoConverter { get; }

		/// <inheritdoc />
		public WotlkToVanillaMovementInfoTypeConverter([NotNull] ITypeConverterProvider<MovementFlag, MovementFlags_Vanilla> moveFlagsConverter, [NotNull] ITypeConverterProvider<TransportationInfo, TransportationInfo_Vanilla> transportInfoConverter)
		{
			MoveFlagsConverter = moveFlagsConverter ?? throw new ArgumentNullException(nameof(moveFlagsConverter));
			TransportInfoConverter = transportInfoConverter ?? throw new ArgumentNullException(nameof(transportInfoConverter));
		}

		//This is not inlineable unless they reference the class method directly
		//but one day coreclr may implement partial virtualcall inlining so we will
		//leave these here anyway.
		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MovementInfo_Vanilla Convert(MovementInfo fromObj)
		{
			if(fromObj == null) return null;

			MovementFlags_Vanilla moveFlags = MoveFlagsConverter.Convert(fromObj.MoveFlags);

			MovementInfo_Vanilla info = new MovementInfo_Vanilla(moveFlags, fromObj.TimeStamp, fromObj.Position,
				fromObj.Orientation, TransportInfoConverter .Convert(fromObj.TransportationInformation), fromObj.TransportationTime,
				fromObj.MovePitch, fromObj.FallTime, fromObj.FallData, fromObj.SplineElevation);

			return info;
		}
	}
}
