using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class VanillaToWotlkObjectUpdateCreateObjectBlockTypeConverter : ITypeConverterProvider<ObjectUpdateCreateObject1Block_Vanilla, ObjectUpdateCreateObject1Block>,
		ITypeConverterProvider<ObjectUpdateCreateObject2Block_Vanilla, ObjectUpdateCreateObject2Block>
	{
		private ITypeConverterProvider<MovementBlockData_Vanilla, MovementBlockData> MoveDataBlockConverter { get; }

		private ITypeConverterProvider<ObjectUpdateValuesObjectBlock_Vanilla, ObjectUpdateValuesObjectBlock> UpdateBlockConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkObjectUpdateCreateObjectBlockTypeConverter([NotNull] ITypeConverterProvider<MovementBlockData_Vanilla, MovementBlockData> moveDataBlockConverter, [NotNull] ITypeConverterProvider<ObjectUpdateValuesObjectBlock_Vanilla, ObjectUpdateValuesObjectBlock> updateBlockConverter)
		{
			MoveDataBlockConverter = moveDataBlockConverter ?? throw new ArgumentNullException(nameof(moveDataBlockConverter));
			UpdateBlockConverter = updateBlockConverter ?? throw new ArgumentNullException(nameof(updateBlockConverter));
		}

		/// <inheritdoc />
		public ObjectUpdateCreateObject1Block Convert(ObjectUpdateCreateObject1Block_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			//TODO: This is kinda hacky, we create an update value block just to convert it
			ObjectUpdateValuesObjectBlock updateBlock = UpdateBlockConverter.Convert(new ObjectUpdateValuesObjectBlock_Vanilla(fromObject.CreationData.CreationGuid, fromObject.CreationData.ObjectValuesCollection));

			//TODO: Once we support update values for all types we should not return null.
			if(updateBlock == null)
				return null;

			return new ObjectUpdateCreateObject1Block(new ObjectCreationData(fromObject.CreationData.CreationGuid, fromObject.CreationData.CreationObjectType,
				MoveDataBlockConverter.Convert(fromObject.CreationData.MovementData), updateBlock.UpdateValuesCollection));
		}

		/// <inheritdoc />
		public ObjectUpdateCreateObject2Block Convert(ObjectUpdateCreateObject2Block_Vanilla fromObject)
		{
			if(fromObject == null) return null;

			//TODO: This is kinda hacky, we create an update value block just to convert it
			ObjectUpdateValuesObjectBlock updateBlock = UpdateBlockConverter.Convert(new ObjectUpdateValuesObjectBlock_Vanilla(fromObject.CreationData.CreationGuid, fromObject.CreationData.ObjectValuesCollection));

			//TODO: Once we support update values for all types we should not return null.
			if(updateBlock == null)
				return null;

			return new ObjectUpdateCreateObject2Block(new ObjectCreationData(fromObject.CreationData.CreationGuid, fromObject.CreationData.CreationObjectType,
				MoveDataBlockConverter.Convert(fromObject.CreationData.MovementData), updateBlock.UpdateValuesCollection));
		}
	}
}
