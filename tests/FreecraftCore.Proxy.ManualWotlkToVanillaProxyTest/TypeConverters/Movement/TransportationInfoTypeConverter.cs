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
	/// Type converter for <see cref="TransportationInfo"/> and <see cref="TransportationInfo_Vanilla"/>
	/// </summary>
	public sealed class TransportationInfoTypeConverter : ITypeConverterProvider<TransportationInfo, TransportationInfo_Vanilla>,
		ITypeConverterProvider<TransportationInfo_Vanilla, TransportationInfo>
	{
		public TransportationInfoTypeConverter()
		{
			
		}

		//This is not inlineable unless they reference the class method directly
		//but one day coreclr may implement partial virtualcall inlining so we will
		//leave these here anyway.
		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TransportationInfo_Vanilla Convert([NotNull] TransportationInfo fromObject)
		{
			if(fromObject == null) return null;

			return new TransportationInfo_Vanilla(new ObjectGuid(fromObject.TransportGuid.RawGuidValue), fromObject.TransportOffset);
		}

		//This is not inlineable unless they reference the class method directly
		//but one day coreclr may implement partial virtualcall inlining so we will
		//leave these here anyway.
		/// <inheritdoc />
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TransportationInfo Convert([NotNull] TransportationInfo_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			//TODO: Are these good default values?
			return new TransportationInfo(new PackedGuid(fromObject.TransportGuid), fromObject.TransportOffset, 0, 1);
		}
	}
}
