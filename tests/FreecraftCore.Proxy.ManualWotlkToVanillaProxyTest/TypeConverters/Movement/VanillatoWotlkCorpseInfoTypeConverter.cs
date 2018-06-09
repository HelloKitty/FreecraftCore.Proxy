using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore
{
	public sealed class VanillatoWotlkCorpseInfoTypeConverter : ITypeConverterProvider<CorpseInfo_Vanilla, CorpseInfo>
	{
		/// <inheritdoc />
		public CorpseInfo Convert(CorpseInfo_Vanilla fromObj)
		{
			if(fromObj == null) return null;

			//TODO: These values are basically all wrong. What can we do?
			return new CorpseInfo(PackedGuid.Empty, fromObj.GoLocation, new Vector3<float>(0, 0, 0), 
				fromObj.Orientation, fromObj.Orientation);
		}
	}
}
