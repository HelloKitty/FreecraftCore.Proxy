using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore
{
	public sealed class VanillaToWotlkObjectUpdateDestroyObjectBlockTypeConverter : ITypeConverterProvider<ObjectUpdateDestroyObjectBlock_Vanilla, ObjectUpdateDestroyObjectBlock>
	{
		/// <inheritdoc />
		public ObjectUpdateDestroyObjectBlock Convert(ObjectUpdateDestroyObjectBlock_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			//This one is simple, it's just the same packed guid collection.
			return new ObjectUpdateDestroyObjectBlock(fromObject.DestroyedGuids);
		}
	}
}
