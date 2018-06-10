using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore
{
	public sealed class CreatureQueryResponseInfoTypeConverter : ITypeConverterProvider<CreatureQueryResponseInfo_Vanilla, CreatureQueryResponseInfo>
	{
		/// <inheritdoc />
		public CreatureQueryResponseInfo Convert(CreatureQueryResponseInfo_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			//TODO: When we have the ctor implement this.
			return new CreatureQueryResponseInfo(fromObject.CreatureNames, fromObject.AlternativeName, "Default", fromObject.Flags, fromObject.CreatureType,
				 fromObject.Family, fromObject.Classification, new int[2]{ 0, 0}, new int[4] { fromObject.CreatureDisplayId, 0, 0, 0}, 1.0f, 1.0f, fromObject.IsLeader, new int[6] {0, 0, 0, 0, 0, 0}, 0);
		}
	}
}
