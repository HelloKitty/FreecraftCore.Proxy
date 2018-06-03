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
		public override Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SMSG_UPDATE_OBJECT_Payload_Vanilla payload)
		{
			ObjectUpdateBlock[] blocks = VanillaToWotlkCompressedUpdatePacketHandler.BuildWotlkUpdateBlock(payload.UpdateBlocks);

			return context.ProxyConnection.SendMessage(new SMSG_UPDATE_OBJECT_Payload(new UpdateBlockCollection(blocks)));
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
		public override Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SMSG_COMPRESSED_UPDATE_OBJECT_Payload_Vanilla payload)
		{
			ObjectUpdateBlock[] wotlkUpdateBlocks = BuildWotlkUpdateBlock(payload.UpdateBlocks);

			//Once the blocks are rebuilt we can send the packet off on its way
			return context.ProxyConnection.SendMessage(new SMSG_COMPRESSED_UPDATE_OBJECT_Payload(new UpdateBlockCollection(wotlkUpdateBlocks)));
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
					return null;
					//return RebuildUpdateValuesBlock(vanillaUpdateBlock as ObjectUpdateValuesObjectBlock_Vanilla);
					break;
				case ObjectUpdateType.UPDATETYPE_MOVEMENT:
					return null;
					//return RebuildMovementBlock(vanillaUpdateBlock as ObjectUpdateMovementBlock_Vanilla);
					break;
				case ObjectUpdateType.UPDATETYPE_CREATE_OBJECT:
					return RebuildCreateObjectBlock1(vanillaUpdateBlock as ObjectUpdateCreateObject1Block_Vanilla);
					break;
				case ObjectUpdateType.UPDATETYPE_CREATE_OBJECT2:
					return RebuildCreateObjectBlock2(vanillaUpdateBlock as ObjectUpdateCreateObject2Block_Vanilla);
					break;
				case ObjectUpdateType.UPDATETYPE_OUT_OF_RANGE_OBJECTS:
					return null;
					//return RebuildOutOfRangeBlock(vanillaUpdateBlock as ObjectUpdateDestroyObjectBlock_Vanilla);
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
			if(objectUpdateCreateObject2BlockVanilla.CreationData.CreationObjectType != ObjectType.Player)
				return null;

			return new ObjectUpdateCreateObject2Block(new ObjectCreationData(objectUpdateCreateObject2BlockVanilla.CreationData.CreationGuid, objectUpdateCreateObject2BlockVanilla.CreationData.CreationObjectType,
				BuildWotlkMovementDataFromVanilla(objectUpdateCreateObject2BlockVanilla.CreationData.MovementData), ToWotlkUpdateValues(objectUpdateCreateObject2BlockVanilla.CreationData.CreationGuid, objectUpdateCreateObject2BlockVanilla.CreationData.ObjectValuesCollection)));
		}

		private static ObjectUpdateBlock RebuildCreateObjectBlock1(ObjectUpdateCreateObject1Block_Vanilla objectUpdateCreateObject1BlockVanilla)
		{
			if(objectUpdateCreateObject1BlockVanilla.CreationData.CreationObjectType != ObjectType.Player)
				return null;

			return new ObjectUpdateCreateObject2Block(new ObjectCreationData(objectUpdateCreateObject1BlockVanilla.CreationData.CreationGuid, objectUpdateCreateObject1BlockVanilla.CreationData.CreationObjectType,
				BuildWotlkMovementDataFromVanilla(objectUpdateCreateObject1BlockVanilla.CreationData.MovementData), ToWotlkUpdateValues(objectUpdateCreateObject1BlockVanilla.CreationData.CreationGuid, objectUpdateCreateObject1BlockVanilla.CreationData.ObjectValuesCollection)));
		}

		private static UpdateFieldValueCollection ToWotlkUpdateValues(BaseGuid objectGuid, UpdateFieldValueCollection updateCollection)
		{
			if(objectGuid.isType(EntityGuidMask.Player))
			{
				//We need to build a new dictionary of update values because the value array could likely change too
				BitArray bitMaskPlayer = new BitArray((int)EUnitFields.PLAYER_END * 32, false);
				Dictionary<int, int> ValuesDictionary = new Dictionary<int, int>(updateCollection.UpdateDiffValues.Length / sizeof(int));

				for(int i = 0, valueIndex = 0; i < updateCollection.UpdateMask.Length; i++)
				{
					//TODO: Blacklist these fields
					//UNIT_FIELD_PERSUADED and UNIT_FIELD_PERSUADED +1
					//

					//After critter the default shift is 2
					int shiftAmount = 2;
					bool shouldWrite = true;

					//This is aligned
					if(i == (int)EUnitFields_Vanilla.UNIT_FIELD_CHANNEL_OBJECT || i == (int)EUnitFields_Vanilla.UNIT_FIELD_CHANNEL_OBJECT + 1)
						shiftAmount = 0;
					else if(i == (int)EUnitFields_Vanilla.UNIT_FIELD_BYTES_0)
					{
						shiftAmount = (int)EUnitFields.UNIT_FIELD_BYTES_0 - (int)EUnitFields_Vanilla.UNIT_FIELD_BYTES_0;
					}
					else if(i == (int)EUnitFields_Vanilla.UNIT_CHANNEL_SPELL)
					{
						shiftAmount = 0x0010 - 0x8a;
					}
					else if(i < (int)EUnitFields.UNIT_FIELD_CRITTER)
					{
						//TODO: This is kinda hacky, it was added after the default shift value
						shiftAmount = 0;
					}
					else if(i >= (int)EUnitFields_Vanilla.UNIT_FIELD_MAXHEALTH && i < (int)EUnitFields_Vanilla.UNIT_FIELD_LEVEL) //keep less than
					{
						shiftAmount = (int)EUnitFields.UNIT_FIELD_MAXHEALTH - (int)EUnitFields_Vanilla.UNIT_FIELD_MAXHEALTH;
					}
					else if(i >= (int)EUnitFields_Vanilla.UNIT_FIELD_LEVEL && i <= (int)EUnitFields_Vanilla.UNIT_FIELD_FACTIONTEMPLATE)
					{
						shiftAmount = (int)EUnitFields.UNIT_FIELD_LEVEL - (int)EUnitFields_Vanilla.UNIT_FIELD_LEVEL;
					}
					else if(i >= (int)EUnitFields_Vanilla.UNIT_VIRTUAL_ITEM_INFO && i <= (int)EUnitFields_Vanilla.UNIT_VIRTUAL_ITEM_INFO_05)
					{
						//Wotlk doesn't have this.
						shouldWrite = false;
					}
					else if(i == (int)EUnitFields_Vanilla.UNIT_FIELD_FLAGS)
					{
						shiftAmount = (int)EUnitFields.UNIT_FIELD_FLAGS - (int)EUnitFields_Vanilla.UNIT_FIELD_FLAGS;
					}
					else if(i >= (int)EUnitFields_Vanilla.UNIT_FIELD_AURA && i <= (int)EUnitFields_Vanilla.UNIT_FIELD_AURAAPPLICATIONS_LAST)
					{
						//Wotlk does not have all this aura stuff
						//It DOES have aura state though.
						shouldWrite = false;
					}
					else if(i >= (int)EUnitFields_Vanilla.UNIT_FIELD_AURASTATE && i <= (int)EUnitFields_Vanilla.UNIT_FIELD_BASEATTACKTIME)
					{
						shiftAmount = 0x0037 - 0x77;
					}
					else if(i == (int)EUnitFields_Vanilla.UNIT_FIELD_OFFHANDATTACKTIME)
					{
						shouldWrite = false;
					}
					else if(i >= (int)EUnitFields_Vanilla.UNIT_FIELD_RANGEDATTACKTIME && i <= (int)EUnitFields_Vanilla.UNIT_DYNAMIC_FLAGS)
					{
						shiftAmount = 0x003A - 0x7a;
					}
					else if(i >= (int)EUnitFields_Vanilla.UNIT_MOD_CAST_SPEED && i <= (int)EUnitFields_Vanilla.UNIT_FIELD_STAT4)
					{
						shiftAmount = 0x004A - 0x8b;
					}
					else if(i >= (int)EUnitFields_Vanilla.UNIT_FIELD_RESISTANCES && i <= (int)EUnitFields_Vanilla.UNIT_FIELD_RESISTANCES_06)
					{
						shiftAmount = 0x005D - 0x95;
					}
					else if(i >= (int)EUnitFields_Vanilla.UNIT_FIELD_BASE_MANA && i <= (int)EUnitFields_Vanilla.UNIT_FIELD_POWER_COST_MULTIPLIER_06)
					{
						shiftAmount = 0x0072 - 0x9c;
					}
					else if(i >= (int)EUnitFields_Vanilla.UNIT_FIELD_PADDING && i <= (int)EUnitFields_Vanilla.PLAYER_GUILD_TIMESTAMP)
					{
						shiftAmount = 0x008D - 0xb5;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_QUEST_LOG_1_1 && i <= (int)EUnitFields_Vanilla.PLAYER_VISIBLE_ITEM_LAST_PAD)
					{
						//TODO: Handle quests and visible item fields
						shouldWrite = false;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_FIELD_INV_SLOT_HEAD && i <= (int)EUnitFields_Vanilla.PLAYER_FIELD_BANK_SLOT_LAST)
					{
						//Only until the last bank slot. Wotlk doesn't have the same bankslot count.
						shiftAmount = 0x00B0 - 0x12a;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_FIELD_VENDORBUYBACK_SLOT_1 && i <= (int)EUnitFields_Vanilla.PLAYER_FARSIGHT)
					{
						shiftAmount = 0x0144 - 0x1b4;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_XP && i <= (int)EUnitFields_Vanilla.PLAYER_PARRY_PERCENTAGE)
					{
						//Only until parry. Because after that wotlk expects expertise which vanilla doesn't have
						shiftAmount = 0x01E6 - 0x210;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_CRIT_PERCENTAGE && i <= (int)EUnitFields_Vanilla.PLAYER_RANGED_CRIT_PERCENTAGE)
					{
						//Only until PLAYER_RANGED_CRIT_PERCENTAGE, wotlk has a couple of extra crit fields
						shiftAmount = 0x0371 - 0x399;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_EXPLORED_ZONES_1 && i < (int)EUnitFields_Vanilla.PLAYER_REST_STATE_EXPERIENCE)
					{
						//Don't include rested
						//We must map the explored
						//Explored for Wotlk is UNIT_END + 0x037D
						//but in vanilla it is 0x39b + UNIT_END
						shiftAmount = 0x037D - 0x39b;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_REST_STATE_EXPERIENCE && i <= (int)EUnitFields_Vanilla.PLAYER_FIELD_COINAGE)
					{
						shiftAmount = 0x03FD - 0x3db;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_FIELD_POSSTAT0 && i <= (int)EUnitFields_Vanilla.PLAYER_FIELD_NEGSTAT4)
					{
						//in vanilla these are player only fields
						//but in trinitycore these are unit fields
						//so we need to go WAY back and insert these
						//Don't write pos/neg resist buffs or else a field will be overwritten.
						shiftAmount = (int)EUnitFields.UNIT_FIELD_POSSTAT0 - (int)EUnitFields_Vanilla.PLAYER_FIELD_POSSTAT0;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE && i <= (int)EUnitFields_Vanilla.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE)
					{
						shiftAmount = (int)EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE - (int)EUnitFields_Vanilla.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_FIELD_MOD_DAMAGE_DONE_POS && i <= (int)EUnitFields_Vanilla.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT)
					{
						//Stop after DONE_PCT because healing fields are next in wotlk but vanilla doesn't have them.
						shiftAmount = (int)EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS - (int)EUnitFields_Vanilla.PLAYER_FIELD_MOD_DAMAGE_DONE_POS;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_FIELD_BYTES)
					{
						shiftAmount = (int)EUnitFields.PLAYER_FIELD_BYTES - (int)EUnitFields_Vanilla.PLAYER_FIELD_BYTES;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_FIELD_SESSION_KILLS && i <= (int)EUnitFields_Vanilla.PLAYER_FIELD_LAST_WEEK_RANK)
					{
						//TODO: How should we handle Honor and stuff?
						shouldWrite = false;
					}
					else if(i >= (int)EUnitFields_Vanilla.PLAYER_FIELD_BYTES2 && i < (int)EUnitFields_Vanilla.PLAYER_END)
					{
						//This is the last fields we will write because it goes all the way to combat_rating_20
						//The issue after this, and it may or may not be handled by the time you read this, is that
						//there are many potentially important player fields after this in wotlk.
						//TODO:
						/*
						 *
						PLAYER_FIELD_ARENA_TEAM_INFO_1_1          = UNIT_END + 0x0454, // Size: 21, Type: INT, Flags: PRIVATE
					    PLAYER_FIELD_HONOR_CURRENCY               = UNIT_END + 0x0469, // Size: 1, Type: INT, Flags: PRIVATE
					    PLAYER_FIELD_ARENA_CURRENCY               = UNIT_END + 0x046A, // Size: 1, Type: INT, Flags: PRIVATE
					    PLAYER_FIELD_MAX_LEVEL                    = UNIT_END + 0x046B, // Size: 1, Type: INT, Flags: PRIVATE
					    PLAYER_FIELD_DAILY_QUESTS_1               = UNIT_END + 0x046C, // Size: 25, Type: INT, Flags: PRIVATE
					    PLAYER_RUNE_REGEN_1                       = UNIT_END + 0x0485, // Size: 4, Type: FLOAT, Flags: PRIVATE
					    PLAYER_NO_REAGENT_COST_1                  = UNIT_END + 0x0489, // Size: 3, Type: INT, Flags: PRIVATE
					    PLAYER_FIELD_GLYPH_SLOTS_1                = UNIT_END + 0x048C, // Size: 6, Type: INT, Flags: PRIVATE
					    PLAYER_FIELD_GLYPHS_1                     = UNIT_END + 0x0492, // Size: 6, Type: INT, Flags: PRIVATE
					    PLAYER_GLYPHS_ENABLED                     = UNIT_END + 0x0498, // Size: 1, Type: INT, Flags: PRIVATE
					    PLAYER_PET_SPELL_POWER                    = UNIT_END + 0x0499, // Size: 1, Type: INT, Flags: PRIVATE
					    PLAYER_END
						 */
						shiftAmount = (int)EUnitFields.PLAYER_FIELD_BYTES2 - (int)EUnitFields_Vanilla.PLAYER_FIELD_BYTES2;
					}

					//We need to track the index of nonwritten to wotlk, but written to the vanilla update block,
					//so that we may remove the byte chunk that represents the indicies

					if(updateCollection.UpdateMask[i])
					{
						if(shouldWrite)
						{
							//We store in a dictionary with the value so that it may be written
							//TODO: Store only index so we can do quick memcpy to new values array in the future for perf
							ValuesDictionary.Add(i + shiftAmount, updateCollection.UpdateDiffValues.Reinterpret<int>(valueIndex * sizeof(int)));
						}

						//no matter what the value index should increase. Because
						//otherwise it will get descyned from the new values
						valueIndex++;
					}
				}

				//Stuff not sent by the vanilla server
				/*[18] PLAYER_FIELD_MAX_LEVEL: 80/1.121039E-43
				[18] PLAYER_FIELD_GLYPH_SLOTS_1: 21/2.942727E-44
				[18] PLAYER_FIELD_GLYPH_SLOTS_2: 22/3.082857E-44
				[18] PLAYER_FIELD_GLYPH_SLOTS_3: 23/3.222986E-44
				[18] PLAYER_FIELD_GLYPH_SLOTS_4: 24/3.363116E-44
				[18] PLAYER_FIELD_GLYPH_SLOTS_5: 25/3.503246E-44
				[18] PLAYER_FIELD_GLYPH_SLOTS_6: 26/3.643376E-44
				[18] PLAYER_GLYPHS_ENABLED: 63/8.82818E-44*/
				ValuesDictionary.Add((int)EUnitFields.PLAYER_GLYPHS_ENABLED, 63);
				ValuesDictionary.Add((int)EUnitFields.PLAYER_FIELD_GLYPH_SLOTS_1 + 1, 21);
				ValuesDictionary.Add((int)EUnitFields.PLAYER_FIELD_GLYPH_SLOTS_1 + 2, 22);
				ValuesDictionary.Add((int)EUnitFields.PLAYER_FIELD_GLYPH_SLOTS_1 + 3, 23);
				ValuesDictionary.Add((int)EUnitFields.PLAYER_FIELD_GLYPH_SLOTS_1 + 4, 24);
				ValuesDictionary.Add((int)EUnitFields.PLAYER_FIELD_GLYPH_SLOTS_1 + 5, 25);
				ValuesDictionary.Add((int)EUnitFields.UNIT_FIELD_FLAGS_2, 264192);
				ValuesDictionary.Add((int)EUnitFields.PLAYER_CHOSEN_TITLE, 0);

				int valueArrayIndex = 0;
				int[] values = new int[ValuesDictionary.Count];
				//We have to build the bitmask now
				foreach(var kvp in ValuesDictionary.OrderBy(k => k.Key))
				{
					bitMaskPlayer.Set(kvp.Key, true);
					values[valueArrayIndex] = kvp.Value;
					valueArrayIndex++;
				}

				File.WriteAllLines($"{ValuesDictionary[(int)EObjectFields.OBJECT_FIELD_GUID]}_GUID_" + Guid.NewGuid(), ValuesDictionary.OrderBy(kvp => kvp.Key).Select(kvp => $"[{kvp.Key}] [{(EUnitFields)kvp.Key}] Value: {kvp.Value}/{kvp.Value.Reinterpret().Reinterpret<float>()}"));
					
				return new UpdateFieldValueCollection(bitMaskPlayer, values.Reinterpret());
			}

				//Manually set these fields for test.
				/*bitMaskPlayer.Set((int)EObjectFields.OBJECT_FIELD_GUID, true);
				bitMaskPlayer.Set((int)EObjectFields.OBJECT_FIELD_TYPE, true);
				bitMaskPlayer.Set((int)EObjectFields.OBJECT_FIELD_SCALE_X, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_BYTES_0, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_HEALTH, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_POWER1, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_POWER4, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_MAXHEALTH, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_MAXPOWER1, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_MAXPOWER2, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_MAXPOWER4, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_MAXPOWER7, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_POWER_REGEN_FLAT_MODIFIER, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_LEVEL, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_FACTIONTEMPLATE, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_FLAGS, true);
				bitMaskPlayer.Set((int)EUnitFields.UNIT_FIELD_FLAGS_2, true);
				bitMaskPlayer.Set((int)EUnitFields.PLAYER_FLAGS, true);
				bitMaskPlayer.Set((int)EUnitFields.PLAYER_GLYPHS_ENABLED, true);
				bitMaskPlayer.Set((int)EUnitFields.PLAYER_FIELD_MAX_LEVEL, true);
				bitMaskPlayer.Set((int)EUnitFields.PLAYER_FIELD_GLYPH_SLOTS_1 + 1, true);
				bitMaskPlayer.Set((int)EUnitFields.PLAYER_FIELD_GLYPH_SLOTS_1 + 2, true);
				bitMaskPlayer.Set((int)EUnitFields.PLAYER_FIELD_GLYPH_SLOTS_1 + 3, true);
				bitMaskPlayer.Set((int)EUnitFields.PLAYER_FIELD_GLYPH_SLOTS_1 + 4, true);
				bitMaskPlayer.Set((int)EUnitFields.PLAYER_FIELD_GLYPH_SLOTS_1 + 5, true);
				int[] updateValues = new int[27];
				updateValues[0] = (int)(objectGuid.RawGuidValue & 0xFFFFFFFF);
				updateValues[1] = 25;
				updateValues[2] = 1065353216;
				updateValues[3] = 2306;
				updateValues[4] = 10976;
				updateValues[5] = 7851;
				updateValues[6] = 7851;
				updateValues[7] = 7851;
				updateValues[8] = 7851;
				updateValues[9] = 7851;
				updateValues[10] = 7851;
				updateValues[11] = 7851;
				updateValues[12] = 1092137461;
				updateValues[13] = 60;
				updateValues[14] = 35;
				updateValues[15] = 8;
				updateValues[16] = 264192;
				updateValues[18] = 8;


				//MAX_LVEL
				updateValues[19] = 60;
				updateValues[20] = 21;
				updateValues[21] = 22;
				updateValues[22] = 23;
				updateValues[23] = 24;
				updateValues[24] = 25;
				updateValues[25] = 26;

				updateValues[26] = 63; //player glyphs enabled

				return new UpdateFieldValueCollection(bitMaskPlayer, updateValues.Reinterpret());
			}*/

				//We start with the new size as the original size
				//From there we need to determine the object type and do the shifting
				//of both the size and any unit fields that didn't get synced.
				//Blocksize itself is determined by the bitmasksize divide by bitcount for the fields (32)
				int bitMaskSize = updateCollection.UpdateMask.Length;

			if(objectGuid.isType(EntityGuidMask.Item))
			{
				//Item could be an item or a container
				//the guid mask doesn't give us enough information.
				//the length of the mask does though.
				if(bitMaskSize > (int)EItemFields_Vanilla.ITEM_END + 1)
				{
					//it is a container
					bitMaskSize = (int)EContainerFields.CONTAINER_END;
					int newBlockCount = (bitMaskSize + 31) / 32;
					//(valuesCount + CLIENT_UPDATE_MASK_BITS - 1) / CLIENT_UPDATE_MASK_BITS;
					BitArray newBitMask = new BitArray(newBlockCount * 32, false);

					ReplaceUpdateItemIndicies(updateCollection, newBitMask);

					int offset = (int)EItemFields.ITEM_END - (int)EItemFields_Vanilla.ITEM_END - 2;
					int i = (int)EItemFields_Vanilla.ITEM_END;
					try
					{
						//Containers also have container fields that need to be replaced
						for(; i < updateCollection.UpdateMask.Length && i + offset < newBitMask.Length; i++)
							newBitMask.Set(i + offset, updateCollection.UpdateMask[i]);
					}
					catch(Exception e)
					{
						throw new InvalidOperationException($"Failed to set. Original Length: {updateCollection.UpdateMask.Length} New Length: {newBitMask.Length} i: {i} i with offset: {i + offset}");
						throw;
					}

					return new UpdateFieldValueCollection(newBitMask, updateCollection.UpdateDiffValues);
				}
				else
				{
					//It is just an item
					bitMaskSize = (int)EItemFields.ITEM_END;

					int newBlockCount = (bitMaskSize + 31) / 32;
					//(valuesCount + CLIENT_UPDATE_MASK_BITS - 1) / CLIENT_UPDATE_MASK_BITS;
					BitArray newBitMask = new BitArray(newBlockCount * 32, false);

					ReplaceUpdateItemIndicies(updateCollection, newBitMask);

					return new UpdateFieldValueCollection(newBitMask, updateCollection.UpdateDiffValues);
				}
			}

			return updateCollection;
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
				flags |= ObjectUpdateFlags.UPDATEFLAG_SELF | ObjectUpdateFlags.UPDATEFLAG_STATIONARY_POSITION;

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
			return new ObjectUpdateValuesObjectBlock(objectUpdateValuesObjectBlockVanilla.ObjectToUpdate, objectUpdateValuesObjectBlockVanilla.UpdateValuesCollection);
		}
	}
}
