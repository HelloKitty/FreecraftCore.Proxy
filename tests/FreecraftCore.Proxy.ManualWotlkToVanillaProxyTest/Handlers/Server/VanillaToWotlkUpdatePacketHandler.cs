using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using FreecraftCore;
using JetBrains.Annotations;
using Reinterpret.Net;

/*[ServerPayloadHandler]
public sealed class VanillaToWotlkUpdatePacketHandler : GameServerPacketLoggerServerPayloadHandler<SMSG_COMPRESSED_UPDATE_OBJECT_DTO_PROXY>
{
	/// <inheritdoc />
	public VanillaToWotlkUpdatePacketHandler([NotNull] ILog logger) : base(logger, false)
	{
	}
}

[ServerPayloadHandler]
public class VanillaToWotlkCompressedUpdatePacketHandler : GameServerPacketLoggerServerPayloadHandler<SMSG_UPDATE_OBJECT_DTO_PROXY>
{
	/// <inheritdoc />
	public VanillaToWotlkCompressedUpdatePacketHandler([NotNull] ILog logger) : base(logger, false)
	{
	}
}*/

namespace FreecraftCore
{
	[ServerPayloadHandler]
	public sealed class VanillaToWotlkUpdatePacketHandler : BaseGameServerPayloadHandler<SMSG_UPDATE_OBJECT_Payload_Vanilla>
	{
		/// <inheritdoc />
		public VanillaToWotlkUpdatePacketHandler([NotNull] ILog logger) : base(logger)
		{
		}

		/// <inheritdoc />
		public override async Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SMSG_UPDATE_OBJECT_Payload_Vanilla payload)
		{
			ObjectUpdateBlock[] blocks = VanillaToWotlkCompressedUpdatePacketHandler.BuildWotlkUpdateBlock(payload.UpdateBlocks);

			await context.ProxyConnection.SendMessage(new SMSG_UPDATE_OBJECT_Payload(new UpdateBlockCollection(blocks)));
		}
	}

	[ServerPayloadHandler]
	public class VanillaToWotlkCompressedUpdatePacketHandler : BaseGameServerPayloadHandler<SMSG_COMPRESSED_UPDATE_OBJECT_Payload_Vanilla>
	{
		/// <inheritdoc />
		public VanillaToWotlkCompressedUpdatePacketHandler([NotNull] ILog logger) : base(logger)
		{

		}

		/// <inheritdoc />
		public override async Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SMSG_COMPRESSED_UPDATE_OBJECT_Payload_Vanilla payload)
		{
			ObjectUpdateBlock[] wotlkUpdateBlocks = BuildWotlkUpdateBlock(payload.UpdateBlocks);

			//Once the blocks are rebuilt we can send the packet off on its way
			await context.ProxyConnection.SendMessage(new SMSG_COMPRESSED_UPDATE_OBJECT_Payload(new UpdateBlockCollection(wotlkUpdateBlocks)));

			/*BitArray postUpdateBitArray = new BitArray((int)(int)1344, false);
			/*[0] UpdateType: Values
			[0] GUID: Full: 0x0000000B Type: Player Low: 11 Name: Testchamp
			[0] UNIT_FIELD_AURASTATE: 4194304/5.877472E-39
			[0] UNIT_DYNAMIC_FLAGS: 0/0
			[0] UNIT_NPC_FLAGS: 0/0#2##1#
			postUpdateBitArray.Set((int)EUnitFields.UNIT_FIELD_AURASTATE, true);
			postUpdateBitArray.Set((int)EUnitFields.UNIT_DYNAMIC_FLAGS, true);
			postUpdateBitArray.Set((int)EUnitFields.UNIT_NPC_FLAGS, true);

			int[] values = new int[3] { 4194304, 0, 0 };

			//This I think 3.3.5 sends but 1.12.1 does not
			ObjectUpdateValuesObjectBlock updateBlock = new ObjectUpdateValuesObjectBlock(new PackedGuid(0x00000001), new UpdateFieldValueCollection(postUpdateBitArray, values.Reinterpret()));

			await context.ProxyConnection.SendMessage(new SMSG_COMPRESSED_UPDATE_OBJECT_Payload(new UpdateBlockCollection(new ObjectUpdateBlock[] {updateBlock})));*/
		}

		public static ObjectUpdateBlock[] BuildWotlkUpdateBlock(UpdateBlockCollection_Vanilla blocks)
		{
			//This is complicated. We need to transform the vanilla version to the wotlk version.
			ObjectUpdateBlock[] wotlkUpdateBlocks = new ObjectUpdateBlock[blocks.Items.Count];

			for(int i = 0; i < blocks.Items.Count; i++)
				wotlkUpdateBlocks[i] = RebuildUpdateBlock(blocks.Items.ElementAt(i));
			return wotlkUpdateBlocks.Where(b => b != null).ToArray();
		}

		public static ObjectUpdateBlock RebuildUpdateBlock([NotNull] ObjectUpdateBlock_Vanilla vanillaUpdateBlock)
		{
			if(vanillaUpdateBlock == null) throw new ArgumentNullException(nameof(vanillaUpdateBlock));

			switch(vanillaUpdateBlock.UpdateType)
			{
				case ObjectUpdateType.UPDATETYPE_VALUES:
					return RebuildUpdateValuesBlock(vanillaUpdateBlock as ObjectUpdateValuesObjectBlock_Vanilla);
					break;
				case ObjectUpdateType.UPDATETYPE_MOVEMENT:
					return RebuildMovementBlock(vanillaUpdateBlock as ObjectUpdateMovementBlock_Vanilla);
					break;
				case ObjectUpdateType.UPDATETYPE_CREATE_OBJECT:
					return RebuildCreateObjectBlock1(vanillaUpdateBlock as ObjectUpdateCreateObject1Block_Vanilla);
					break;
				case ObjectUpdateType.UPDATETYPE_CREATE_OBJECT2:
					return RebuildCreateObjectBlock2(vanillaUpdateBlock as ObjectUpdateCreateObject2Block_Vanilla);
					break;
				case ObjectUpdateType.UPDATETYPE_OUT_OF_RANGE_OBJECTS:
					return RebuildOutOfRangeBlock(vanillaUpdateBlock as ObjectUpdateDestroyObjectBlock_Vanilla);
					break;
				default:
					throw new InvalidOperationException($"Cannot create block for BlockType: {vanillaUpdateBlock.UpdateType}");
			}
		}

		private static ObjectUpdateDestroyObjectBlock RebuildOutOfRangeBlock(ObjectUpdateDestroyObjectBlock_Vanilla objectUpdateDestroyObjectBlockVanilla)
		{
			//This one is simple, it's just the same packed guid collection.
			return new ObjectUpdateDestroyObjectBlock(objectUpdateDestroyObjectBlockVanilla.DestroyedGuids);
		}

		private static ObjectUpdateCreateObject2Block RebuildCreateObjectBlock2(ObjectUpdateCreateObject2Block_Vanilla objectUpdateCreateObject2BlockVanilla)
		{
			if(objectUpdateCreateObject2BlockVanilla.CreationData.CreationObjectType != ObjectType.Player && objectUpdateCreateObject2BlockVanilla.CreationData.CreationObjectType != ObjectType.Unit)
				return null;

			return new ObjectUpdateCreateObject2Block(new ObjectCreationData(objectUpdateCreateObject2BlockVanilla.CreationData.CreationGuid, objectUpdateCreateObject2BlockVanilla.CreationData.CreationObjectType,
				BuildWotlkMovementDataFromVanilla(objectUpdateCreateObject2BlockVanilla.CreationData.MovementData), ToWotlkUpdateValues(objectUpdateCreateObject2BlockVanilla.CreationData.CreationGuid, objectUpdateCreateObject2BlockVanilla.CreationData.ObjectValuesCollection)));
		}

		private static ObjectUpdateCreateObject1Block RebuildCreateObjectBlock1(ObjectUpdateCreateObject1Block_Vanilla objectUpdateCreateObject1BlockVanilla)
		{
			if(objectUpdateCreateObject1BlockVanilla.CreationData.CreationObjectType != ObjectType.Player && objectUpdateCreateObject1BlockVanilla.CreationData.CreationObjectType != ObjectType.Unit)
				return null;

			return new ObjectUpdateCreateObject1Block(new ObjectCreationData(objectUpdateCreateObject1BlockVanilla.CreationData.CreationGuid, objectUpdateCreateObject1BlockVanilla.CreationData.CreationObjectType,
				BuildWotlkMovementDataFromVanilla(objectUpdateCreateObject1BlockVanilla.CreationData.MovementData), ToWotlkUpdateValues(objectUpdateCreateObject1BlockVanilla.CreationData.CreationGuid, objectUpdateCreateObject1BlockVanilla.CreationData.ObjectValuesCollection)));
		}

		private static UpdateFieldValueCollection ToWotlkUpdateValues(BaseGuid objectGuid, UpdateFieldValueCollection updateCollection)
		{
			if(objectGuid.isType(EntityGuidMask.Player))
			{
				return BuildWotlkPlayerUpdateFieldCollection(updateCollection);
			}
			else if(objectGuid.isType(EntityGuidMask.Unit))
				return BuildWotlkUnitUpdateFieldCollection(updateCollection);


			return null;
		}

		private static UpdateFieldValueCollection BuildWotlkUnitUpdateFieldCollection(UpdateFieldValueCollection updateCollection)
		{
			//We need to build a new dictionary of update values because the value array could likely change too
			//TODO: Don't hardcode size value, compute block size manually
			BitArray bitMaskPlayer = new BitArray((int)192, false); //TODO: What is the unit size? Is UNIT_END correct?
			Dictionary<int, int> ValuesDictionary = new Dictionary<int, int>(updateCollection.UpdateDiffValues.Length / sizeof(int));
			int valueIndex = 0;

			InitializeObjectFields(updateCollection, ValuesDictionary, ref valueIndex);
			InitializeUnitFields(updateCollection, ValuesDictionary, ref valueIndex, EUnitFields_Vanilla.UNIT_END);

			int[] values = SetBitMaskAndBuildValues(bitMaskPlayer, ValuesDictionary);

			return new UpdateFieldValueCollection(bitMaskPlayer, values.Reinterpret());
		}

		private static UpdateFieldValueCollection BuildWotlkPlayerUpdateFieldCollection(UpdateFieldValueCollection updateCollection)
		{
			//We need to build a new dictionary of update values because the value array could likely change too
			//TODO: Don't hardcode size value, compute block size manually
			BitArray bitMaskPlayer = new BitArray((int)1344, false);
			Dictionary<int, int> ValuesDictionary = new Dictionary<int, int>(updateCollection.UpdateDiffValues.Length / sizeof(int));
			int valueIndex = 0;

			InitializeObjectFields(updateCollection, ValuesDictionary, ref valueIndex);

			InitializeUnitFields(updateCollection, ValuesDictionary, ref valueIndex);

			if(!ValuesDictionary.ContainsKey((int)EUnitFields.UNIT_FIELD_FLAGS_2)) ValuesDictionary.Add((int)EUnitFields.UNIT_FIELD_FLAGS_2, 264192);
			if(!ValuesDictionary.ContainsKey((int)EUnitFields.UNIT_FIELD_HOVERHEIGHT)) ValuesDictionary.Add((int)EUnitFields.UNIT_FIELD_HOVERHEIGHT, 1.0f.Reinterpret().Reinterpret<int>()); //TODO: change to constant
			if(!ValuesDictionary.ContainsKey((int)EUnitFields.UNIT_FIELD_POWER_REGEN_FLAT_MODIFIER)) ValuesDictionary.Add((int)EUnitFields.UNIT_FIELD_POWER_REGEN_FLAT_MODIFIER, 1082423867);
			if(!ValuesDictionary.ContainsKey((int)EUnitFields.PLAYER_FIELD_MAX_LEVEL)) ValuesDictionary.Add((int)EUnitFields.PLAYER_FIELD_MAX_LEVEL, 60);

			int[] values = SetBitMaskAndBuildValues(bitMaskPlayer, ValuesDictionary);

			return new UpdateFieldValueCollection(bitMaskPlayer, values.Reinterpret());
		}

		private static void InitializeUnitFields(UpdateFieldValueCollection updateCollection, Dictionary<int, int> valuesDictionary, ref int valueIndex, EUnitFields_Vanilla endField = EUnitFields_Vanilla.PLAYER_END)
		{
			//subtract 1 from the length size because it's PlayerEnd
			for(int i = (int)EObjectFields.OBJECT_END; i < updateCollection.UpdateMask.Length && i < (int)endField; i++)
			{
				//TODO: Blacklist these fields
				//UNIT_FIELD_PERSUADED and UNIT_FIELD_PERSUADED +1
				//UNIT_CHANNEL_SPELL

				bool shouldWrite = VanillaToWotlkConverter.ConvertUpdateFieldsPlayer((EUnitFields_Vanilla)i, out int shiftAmount);

				//We need to track the index of nonwritten to wotlk, but written to the vanilla update block,
				//so that we may remove the byte chunk that represents the indicies
				if(updateCollection.UpdateMask[i])
				{
					if(shouldWrite)
					{
						try
						{
							//We store in a dictionary with the value so that it may be written
							//TODO: Store only index so we can do quick memcpy to new values array in the future for perf
							valuesDictionary.Add(i + shiftAmount, updateCollection.UpdateDiffValues.Reinterpret<int>(valueIndex * sizeof(int)));
						}
						catch(Exception e)
						{
							throw new InvalidOperationException($"Failed to insert: i:{i}:{((EUnitFields_Vanilla)i).ToString()} [{i + shiftAmount}] [{((EUnitFields)(i + shiftAmount)).ToString()}] {updateCollection.UpdateDiffValues.Reinterpret<int>(valueIndex * sizeof(int))} into dictionary. \n\n Exception: {e.Message}", e);
						}
					}

					//no matter what the value index should increase. Because
					//otherwise it will get descyned from the new values
					valueIndex++;
				}
			}
		}

		private static void InitializeObjectFields(UpdateFieldValueCollection updateCollection, Dictionary<int, int> ValuesDictionary, ref int valueIndex)
		{
			//Wotlk and vanilla both have the same object field
			for(int i = 0; i < (int)EObjectFields.OBJECT_END; i++)
			{
				if(updateCollection.UpdateMask[i])
				{
					ValuesDictionary.Add(i, updateCollection.UpdateDiffValues.Reinterpret<int>(valueIndex * sizeof(int)));
					valueIndex++;
				}
			}
		}

		private static int[] SetBitMaskAndBuildValues(BitArray bitMaskPlayer, Dictionary<int, int> ValuesDictionary)
		{
			int valueArrayIndex = 0;
			int[] values = new int[ValuesDictionary.Count];
			//We have to build the bitmask now
			foreach(var kvp in ValuesDictionary.OrderBy(k => k.Key))
			{
				bitMaskPlayer.Set(kvp.Key, true);
				values[valueArrayIndex] = kvp.Value;
				valueArrayIndex++;
			}

			//TODO: Remove this debug stuff
			if(ValuesDictionary.ContainsKey((int)EObjectFields.OBJECT_FIELD_GUID))
				File.WriteAllLines($"{ValuesDictionary[(int)EObjectFields.OBJECT_FIELD_GUID]}_GUID_" + Guid.NewGuid(), ValuesDictionary.OrderBy(kvp => kvp.Key).Select(kvp => $"[{kvp.Key}]:[0x{kvp.Key:X}] [{(EUnitFields)kvp.Key}] Value: {kvp.Value}/{kvp.Value.Reinterpret().Reinterpret<float>()}"));

			return values;
		}

		private static void ReplaceUpdateItemIndicies(UpdateFieldValueCollection updateCollection, BitArray newBitMask)
		{
			int postEnchantShiftDifference = (int)EItemFields.ITEM_FIELD_PROPERTY_SEED - (int)EItemFields_Vanilla.ITEM_FIELD_PROPERTY_SEED;

			//We have to go through the collection and clear or wipe some values
			for(int i = 0; i < (int)updateCollection.UpdateMask.Length; i++)
			{
				try
				{
					//This is where items actually shift and get messed up from vanilla to wotlk
					//so we skip these for now and shift forward after it
					if(i < (int)EItemFields_Vanilla.ITEM_FIELD_ENCHANTMENT)
					{
						newBitMask.Set(i, updateCollection.UpdateMask[i]);
					}
					else if(i >= (int)EItemFields_Vanilla.ITEM_FIELD_ENCHANTMENT || i < (int)EItemFields_Vanilla.ITEM_FIELD_PROPERTY_SEED)
					{
						//TODO: Support enchant fields
						//Don't reference the enchant fields
						newBitMask.Set(i, updateCollection.UpdateMask[i]);
					}
					else
					{
						//At this point we are at ITEM_FIELD_PROPERTY_SEED
						//Which means for items we can just shift forward and set
						//the fields
						newBitMask.Set(i + postEnchantShiftDifference, newBitMask[i]);
					}
				}
				catch(Exception e)
				{
					throw new InvalidOperationException($"Failed to set. Original Length: {updateCollection.UpdateMask.Length} New Length: {newBitMask.Length} i: {i} i with offset: {i + postEnchantShiftDifference}");
				}
			}
		}

		private static ObjectUpdateMovementBlock RebuildMovementBlock(ObjectUpdateMovementBlock_Vanilla objectUpdateMovementBlockVanilla)
		{
			//The wotlk version wants this guid packed
			PackedGuid packedMovementGuid = new PackedGuid(objectUpdateMovementBlockVanilla.MovementGuid.RawGuidValue);

			ObjectUpdateMovementBlock wotlkBlock = new ObjectUpdateMovementBlock(packedMovementGuid, BuildWotlkMovementDataFromVanilla(objectUpdateMovementBlockVanilla.MovementData));

			return wotlkBlock;
		}

		private static MovementBlockData BuildWotlkMovementDataFromVanilla(MovementBlockData_Vanilla movementData)
		{
			ObjectUpdateFlags flags = MapVanillaToWotlkMoveUpdateFlags(movementData.UpdateFlags);

			//Should be null if the object isn't living.
			MovementInfo info = flags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_LIVING) ? MapVanillaToWotlkMoveInfo(flags, movementData) : null;
			bool shouldIncludeSplines = flags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_LIVING) && info.MovementFlags.HasFlag(MovementFlag.SplineEnabled);
			bool hasCorpseInfo = !flags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_LIVING) && flags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_POSITION);
			SplineInfo spline = shouldIncludeSplines ? ToWotlkSplineInfo(movementData.SplineInformation) : null;
			CorpseInfo corpseInfo = hasCorpseInfo ? ToWotlkCorpseInfo(movementData.DeadMovementInformation) : null;

			MovementBlockData data = new MovementBlockData(flags, info, movementData.MovementSpeeds?.Concat(Enumerable.Repeat(1.0f, 3))?.ToArray(), spline, corpseInfo, null,
				/*TC always sends 0 but we can try this*/ (int)movementData.HighGuid, /*Mangos always sends for UPDATE_ALL 0x10 AKA LOW_GUID. We can try TC hack if this doesn't work*/ 0x1,
				/*This is target*/ flags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_HAS_TARGET) ? movementData.FullGuid : PackedGuid.Empty, movementData.TransportTime, null, /*This is a packed QUATERRION but Vanilla doesn't send anything. TODO compute this*/ 0);

			return data;
		}

		private static CorpseInfo ToWotlkCorpseInfo(CorpseInfo_Vanilla movementDataDeadMovementInformation)
		{
			//TODO: These values are basically all wrong. What can we do?
			return new CorpseInfo(PackedGuid.Empty, movementDataDeadMovementInformation.GoLocation, new Vector3<float>(0,0,0), movementDataDeadMovementInformation.Orientation, movementDataDeadMovementInformation.Orientation);
		}

		private static SplineInfo ToWotlkSplineInfo(SplineInfo_Vanilla movementDataSplineInformation)
		{
			//TODO: Check these default wotlk values
			return new SplineInfo(ToWotlkSplineFlags(movementDataSplineInformation.SplineFlags), movementDataSplineInformation.FinalTarget, movementDataSplineInformation.FinalOrientation,
				movementDataSplineInformation.FinalPoint, movementDataSplineInformation.SplineTime, movementDataSplineInformation.SplineFullTime, movementDataSplineInformation.SplineId, 1.0f, 1.0f,
				0.0f, 0, movementDataSplineInformation.WayPoints, SplineMode.Linear, movementDataSplineInformation.SplineEndpoint);
		}

		//TODO: This is unfinished
		private static SplineFlag ToWotlkSplineFlags(SplineFlags_Vanilla splineFlags)
		{
			SplineFlag flags = 0;

			if(splineFlags.HasFlag(SplineFlags_Vanilla.Final_Angle))
				flags |= SplineFlag.FinalOrientation;

			if(splineFlags.HasFlag(SplineFlags_Vanilla.Final_Target))
				flags |= SplineFlag.FinalTarget;

			if(splineFlags.HasFlag(SplineFlags_Vanilla.Final_Point))
				flags |= SplineFlag.FinalPoint;

			if(splineFlags.HasFlag(SplineFlags_Vanilla.Done))
				flags |= SplineFlag.Done;

			if(splineFlags.HasFlag(SplineFlags_Vanilla.Cyclic))
				flags |= SplineFlag.Cyclic;

			if(splineFlags.HasFlag(SplineFlags_Vanilla.No_Spline))
				flags |= SplineFlag.NoSpline;

			return flags;
		}

		private static MovementInfo MapVanillaToWotlkMoveInfo(ObjectUpdateFlags flags, MovementBlockData_Vanilla movementData)
		{
			MovementFlag moveFlags = ToWotlkMoveFlags(movementData.MoveInfo.MovementFlags);

			MovementInfo info = new MovementInfo(moveFlags, MovementFlagExtra.None, 
				movementData.MoveInfo.TimeStamp, movementData.MoveInfo.Position,
				0.0f, moveFlags.HasFlag(MovementFlag.OnTransport) ? ToWotlkTransportData(movementData.MoveInfo.TransportationInformation) : null,
				0, movementData.MoveInfo.MovePitch, movementData.MoveInfo.FallTime, movementData.MoveInfo.FallData, movementData.MoveInfo.SplineElevation);

			return info;
		}

		private static TransportationInfo ToWotlkTransportData(TransportationInfo_Vanilla moveInfoTransportationInformation)
		{
			//TODO: Are these good default values?
			return new TransportationInfo(new PackedGuid(moveInfoTransportationInformation.TransportGuid), moveInfoTransportationInformation.TransportOffset, 0, 1);
		}

		private static MovementFlag ToWotlkMoveFlags(MovementFlags_Vanilla moveInfoMovementFlags)
		{
			//First 11 bits mean the same thing
			MovementFlag wotlkMoveFlags = (MovementFlag)((int)moveInfoMovementFlags & 0b0111_1111_1111);

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_ROOT))
				wotlkMoveFlags |= MovementFlag.Root;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_FLYING))
				wotlkMoveFlags |= MovementFlag.Flying;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_FALLING))
				wotlkMoveFlags |= MovementFlag.Falling;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_FALLINGFAR))
				wotlkMoveFlags |= MovementFlag.FallingFar;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_SWIMMING))
				wotlkMoveFlags |= MovementFlag.Swimming;

			//This means we need to provide spline information in the move info
			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_SPLINE_ENABLED))
				wotlkMoveFlags |= MovementFlag.SplineEnabled;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_CAN_FLY))
				wotlkMoveFlags |= MovementFlag.CanFly;

			//Skip flying old
			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_ONTRANSPORT))
				wotlkMoveFlags |= MovementFlag.OnTransport;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_SPLINE_ELEVATION))
				wotlkMoveFlags |= MovementFlag.SplineElevation;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_WATERWALKING))
				wotlkMoveFlags |= MovementFlag.Waterwalking;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_SAFE_FALL))
				wotlkMoveFlags |= MovementFlag.CanSafeFall;

			if(moveInfoMovementFlags.HasFlag(MovementFlags_Vanilla.MOVEFLAG_HOVER))
				wotlkMoveFlags |= MovementFlag.Hover;

			return wotlkMoveFlags;
		}

		private static ObjectUpdateFlags MapVanillaToWotlkMoveUpdateFlags(ObjectUpdateFlags_Vanilla movementDataUpdateFlags)
		{
			ObjectUpdateFlags flags = 0;

			//This means we need to include the 32bit transport time.
			if(movementDataUpdateFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_TRANSPORT))
				flags |= ObjectUpdateFlags.UPDATEFLAG_TRANSPORT;

			if(movementDataUpdateFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_LIVING))
				flags |= ObjectUpdateFlags.UPDATEFLAG_LIVING;

			//I think this means we have to set the transport object if living too.
			if(movementDataUpdateFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_HAS_POSITION))
				flags |= ObjectUpdateFlags.UPDATEFLAG_POSITION;

			//TODO: Remove stationary hack
			if(movementDataUpdateFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_SELF))
				flags |= ObjectUpdateFlags.UPDATEFLAG_SELF;

			//This is odd, but they will send the guid of the target if this flag is enabled
			//So I assume we have to do the same here.
			if(movementDataUpdateFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_FULLGUID))
				flags |= ObjectUpdateFlags.UPDATEFLAG_HAS_TARGET;

			//Mangos sends the highGuid (32bit int) of the current unit this is the update for.
			//But TC just sends 0? So I gurss we should do that. It's unknown either way.
			if(movementDataUpdateFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_HIGHGUID))
				flags |= ObjectUpdateFlags.UPDATEFLAG_UNKNOWN;

			return flags;
		}

		private static ObjectUpdateValuesObjectBlock RebuildUpdateValuesBlock(ObjectUpdateValuesObjectBlock_Vanilla objectUpdateValuesObjectBlockVanilla)
		{
			UpdateFieldValueCollection fieldValueCollection = ToWotlkUpdateValues(objectUpdateValuesObjectBlockVanilla.ObjectToUpdate, objectUpdateValuesObjectBlockVanilla.UpdateValuesCollection);
			return new ObjectUpdateValuesObjectBlock(objectUpdateValuesObjectBlockVanilla.ObjectToUpdate, fieldValueCollection);
		}
	}
}
