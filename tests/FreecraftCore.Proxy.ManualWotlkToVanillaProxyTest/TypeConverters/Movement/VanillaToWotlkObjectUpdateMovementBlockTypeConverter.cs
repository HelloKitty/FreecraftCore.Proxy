using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class VanillaToWotlkObjectUpdateMovementBlockTypeConverter : ITypeConverterProvider<ObjectUpdateMovementBlock_Vanilla, ObjectUpdateMovementBlock>
	{
		private ITypeConverterProvider<MovementBlockData_Vanilla, MovementBlockData> MovementBlockDataConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkObjectUpdateMovementBlockTypeConverter([NotNull] ITypeConverterProvider<MovementBlockData_Vanilla, MovementBlockData> movementBlockDataConverter)
		{
			MovementBlockDataConverter = movementBlockDataConverter ?? throw new ArgumentNullException(nameof(movementBlockDataConverter));
		}

		/// <inheritdoc />
		public ObjectUpdateMovementBlock Convert(ObjectUpdateMovementBlock_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			//The wotlk version wants this guid packed
			PackedGuid packedMovementGuid = new PackedGuid(fromObject.MovementGuid.RawGuidValue);

			ObjectUpdateMovementBlock wotlkBlock = new ObjectUpdateMovementBlock(packedMovementGuid, MovementBlockDataConverter.Convert(fromObject.MovementData));

			return wotlkBlock;
		}
	}
}
