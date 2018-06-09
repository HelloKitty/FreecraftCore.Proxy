using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class VanillaToWotlkObjectUpdateBlock : ITypeConverterProvider<ObjectUpdateBlock_Vanilla, ObjectUpdateBlock>
	{
		private ITypeConverterProvider<ObjectUpdateValuesObjectBlock_Vanilla, ObjectUpdateValuesObjectBlock> UpdateBlockConverter { get; }

		private ITypeConverterProvider<ObjectUpdateMovementBlock_Vanilla, ObjectUpdateMovementBlock> MovementBlockConverter { get; }

		private ITypeConverterProvider<ObjectUpdateCreateObject1Block_Vanilla, ObjectUpdateCreateObject1Block> CreateObject1BlockConverter { get; }

		private ITypeConverterProvider<ObjectUpdateCreateObject2Block_Vanilla, ObjectUpdateCreateObject2Block> CreateObject2BlockConverter { get; }

		private ITypeConverterProvider<ObjectUpdateDestroyObjectBlock_Vanilla, ObjectUpdateDestroyObjectBlock> DestroyBlockConverter { get; }

		/// <inheritdoc />
		public VanillaToWotlkObjectUpdateBlock([NotNull] ITypeConverterProvider<ObjectUpdateValuesObjectBlock_Vanilla, ObjectUpdateValuesObjectBlock> updateBlockConverter, 
			[NotNull] ITypeConverterProvider<ObjectUpdateMovementBlock_Vanilla, ObjectUpdateMovementBlock> movementBlockConverter, 
			[NotNull] ITypeConverterProvider<ObjectUpdateCreateObject1Block_Vanilla, ObjectUpdateCreateObject1Block> object1BlockConverter, 
			[NotNull] ITypeConverterProvider<ObjectUpdateCreateObject2Block_Vanilla, ObjectUpdateCreateObject2Block> object2BlockConverter, 
			[NotNull] ITypeConverterProvider<ObjectUpdateDestroyObjectBlock_Vanilla, ObjectUpdateDestroyObjectBlock> destroyBlockConverter)
		{
			UpdateBlockConverter = updateBlockConverter ?? throw new ArgumentNullException(nameof(updateBlockConverter));
			MovementBlockConverter = movementBlockConverter ?? throw new ArgumentNullException(nameof(movementBlockConverter));
			CreateObject1BlockConverter = object1BlockConverter ?? throw new ArgumentNullException(nameof(object1BlockConverter));
			CreateObject2BlockConverter = object2BlockConverter ?? throw new ArgumentNullException(nameof(object2BlockConverter));
			DestroyBlockConverter = destroyBlockConverter ?? throw new ArgumentNullException(nameof(destroyBlockConverter));
		}

		/// <inheritdoc />
		public ObjectUpdateBlock Convert(ObjectUpdateBlock_Vanilla fromObj)
		{
			if(fromObj == null) return null;

			switch(fromObj.UpdateType)
			{
				case ObjectUpdateType.UPDATETYPE_VALUES:
					return UpdateBlockConverter.Convert(fromObj as ObjectUpdateValuesObjectBlock_Vanilla);
				case ObjectUpdateType.UPDATETYPE_MOVEMENT:
					return MovementBlockConverter.Convert(fromObj as ObjectUpdateMovementBlock_Vanilla);
				case ObjectUpdateType.UPDATETYPE_CREATE_OBJECT:
					return CreateObject1BlockConverter.Convert(fromObj as ObjectUpdateCreateObject1Block_Vanilla);
				case ObjectUpdateType.UPDATETYPE_CREATE_OBJECT2:
					return CreateObject2BlockConverter.Convert(fromObj as ObjectUpdateCreateObject2Block_Vanilla);
				case ObjectUpdateType.UPDATETYPE_OUT_OF_RANGE_OBJECTS:
					return DestroyBlockConverter.Convert(fromObj as ObjectUpdateDestroyObjectBlock_Vanilla);
				default:
					throw new InvalidOperationException($"Cannot create block for BlockType: {fromObj.UpdateType}");
			}
		}
	}
}
