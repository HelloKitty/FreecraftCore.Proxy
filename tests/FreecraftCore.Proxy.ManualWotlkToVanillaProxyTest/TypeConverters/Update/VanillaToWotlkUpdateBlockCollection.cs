using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class VanillaToWotlkUpdateBlockCollection : ITypeConverterProvider<UpdateBlockCollection_Vanilla, UpdateBlockCollection>
	{
		private ITypeConverterProvider<ObjectUpdateBlock_Vanilla, ObjectUpdateBlock> ObjectBlockConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkUpdateBlockCollection([NotNull] ITypeConverterProvider<ObjectUpdateBlock_Vanilla, ObjectUpdateBlock> objectBlockConverter)
		{
			ObjectBlockConverter = objectBlockConverter ?? throw new ArgumentNullException(nameof(objectBlockConverter));
		}

		/// <inheritdoc />
		public UpdateBlockCollection Convert(UpdateBlockCollection_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			//This is complicated. We need to transform the vanilla version to the wotlk version.
			ObjectUpdateBlock[] wotlkUpdateBlocks = new ObjectUpdateBlock[fromObject.Items.Count];

			for(int i = 0; i < fromObject.Items.Count; i++)
				wotlkUpdateBlocks[i] = ObjectBlockConverter.Convert(fromObject.Items.ElementAt(i));

			//TODO: When we are sure we won't have nulls anymore we should remove the where.
			return new UpdateBlockCollection(wotlkUpdateBlocks
				.Where(b => b != null)
				.ToArray());
		}
	}
}
