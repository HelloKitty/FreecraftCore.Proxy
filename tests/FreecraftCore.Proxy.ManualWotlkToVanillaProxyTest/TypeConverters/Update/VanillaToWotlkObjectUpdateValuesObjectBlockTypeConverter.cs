using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;
using Reinterpret.Net;

namespace FreecraftCore
{
	/// <summary>
	/// Type converter from the Vanilla <see cref="ObjectUpdateValuesObjectBlock_Vanilla"/> to the Wotlk <see cref="ObjectUpdateValuesObjectBlock"/>.
	/// </summary>
	public sealed class VanillaToWotlkObjectUpdateValuesObjectBlockTypeConverter : ITypeConverterProvider<ObjectUpdateValuesObjectBlock_Vanilla, ObjectUpdateValuesObjectBlock>
	{
		private ILog Logger { get; }

		public VanillaToWotlkObjectUpdateValuesObjectBlockTypeConverter([NotNull] ILog logger)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public ObjectUpdateValuesObjectBlock Convert(ObjectUpdateValuesObjectBlock_Vanilla fromObj)
		{
			if(fromObj == null) return null;

			UpdateFieldValueCollection fieldValueCollection = ToWotlkUpdateValues(fromObj.ObjectToUpdate, fromObj.UpdateValuesCollection);

			if(fieldValueCollection == null)
				return null;

			return new ObjectUpdateValuesObjectBlock(fromObj.ObjectToUpdate, fieldValueCollection);
		}

		private UpdateFieldValueCollection ToWotlkUpdateValues(PackedGuid objectGuid, UpdateFieldValueCollection updateCollection)
		{
			if(objectGuid.isType(EntityGuidMask.Player))
			{
				return BuildWotlkPlayerUpdateFieldCollection(updateCollection);
			}
			else if(objectGuid.isType(EntityGuidMask.Unit))
				return BuildWotlkUnitUpdateFieldCollection(updateCollection);
			else if(objectGuid.isType(EntityGuidMask.Container))
			{
				if(updateCollection.UpdateMask.Length > 63)
					return BuildWotlkContainerUpdateFieldCollection(updateCollection);
				else
					return BuildWotlkItemUpdateFieldCollection(updateCollection);
			}
			else if(objectGuid.isType(EntityGuidMask.GameObject))
				return BuildWotlkGameObjectUpdateFieldCollection(updateCollection);
			else
				if(Logger.IsWarnEnabled)
					Logger.Warn($"Failed to handle update values for Entity: {objectGuid.RawGuidValue}");

			return null;
		}

		private UpdateFieldValueCollection BuildWotlkGameObjectUpdateFieldCollection(UpdateFieldValueCollection updateCollection)
		{
			//We need to build a new dictionary of update values because the value array could likely change too
			//TODO: Don't hardcode size value, compute block size manually
			BitArray bitMaskPlayer = new BitArray((int)32, false);
			Dictionary<int, int> valuesDictionary = new Dictionary<int, int>(updateCollection.UpdateDiffValues.Length / sizeof(int));
			int valueIndex = 0;

			InitializeObjectFields(updateCollection, valuesDictionary, ref valueIndex);
			InitializeGameObjectFields(updateCollection, valuesDictionary, ref valueIndex);

			int[] values = SetBitMaskAndBuildValues(bitMaskPlayer, valuesDictionary);

			return new UpdateFieldValueCollection(bitMaskPlayer, values.Reinterpret());
		}

		private UpdateFieldValueCollection BuildWotlkItemUpdateFieldCollection(UpdateFieldValueCollection updateCollection)
		{
			//We need to build a new dictionary of update values because the value array could likely change too
			//TODO: Don't hardcode size value, compute block size manually
			BitArray bitMaskPlayer = new BitArray((int)64, false);
			Dictionary<int, int> valuesDictionary = new Dictionary<int, int>(updateCollection.UpdateDiffValues.Length / sizeof(int));
			int valueIndex = 0;

			InitializeObjectFields(updateCollection, valuesDictionary, ref valueIndex);
			InitializeItemFields(updateCollection, valuesDictionary, ref valueIndex);

			int[] values = SetBitMaskAndBuildValues(bitMaskPlayer, valuesDictionary);

			return new UpdateFieldValueCollection(bitMaskPlayer, values.Reinterpret());
		}

		private UpdateFieldValueCollection BuildWotlkContainerUpdateFieldCollection(UpdateFieldValueCollection updateCollection)
		{
			//We need to build a new dictionary of update values because the value array could likely change too
			//TODO: Don't hardcode size value, compute block size manually
			BitArray bitMaskPlayer = new BitArray((int)128, false);
			Dictionary<int, int> valuesDictionary = new Dictionary<int, int>(updateCollection.UpdateDiffValues.Length / sizeof(int));
			int valueIndex = 0;

			InitializeObjectFields(updateCollection, valuesDictionary, ref valueIndex);
			InitializeItemFields(updateCollection, valuesDictionary, ref valueIndex);
			InitializeContainerFields(updateCollection, valuesDictionary, ref valueIndex);

			int[] values = SetBitMaskAndBuildValues(bitMaskPlayer, valuesDictionary);

			return new UpdateFieldValueCollection(bitMaskPlayer, values.Reinterpret());
		}

		private static UpdateFieldValueCollection BuildWotlkPlayerUpdateFieldCollection(UpdateFieldValueCollection updateCollection)
		{
			//We need to build a new dictionary of update values because the value array could likely change too
			//TODO: Don't hardcode size value, compute block size manually
			BitArray bitMaskPlayer = new BitArray((int)1344, false);
			Dictionary<int, int> valuesDictionary = new Dictionary<int, int>(updateCollection.UpdateDiffValues.Length / sizeof(int));
			int valueIndex = 0;

			InitializeObjectFields(updateCollection, valuesDictionary, ref valueIndex);
			InitializeUnitFields(updateCollection, valuesDictionary, ref valueIndex);

			//if(!ValuesDictionary.ContainsKey((int)EUnitFields.UNIT_FIELD_FLAGS_2)) ValuesDictionary.Add((int)EUnitFields.UNIT_FIELD_FLAGS_2, 264192);
			//if(!ValuesDictionary.ContainsKey((int)EUnitFields.UNIT_FIELD_HOVERHEIGHT)) ValuesDictionary.Add((int)EUnitFields.UNIT_FIELD_HOVERHEIGHT, 1.0f.Reinterpret().Reinterpret<int>()); //TODO: change to constant
			//if(!ValuesDictionary.ContainsKey((int)EUnitFields.UNIT_FIELD_POWER_REGEN_FLAT_MODIFIER)) ValuesDictionary.Add((int)EUnitFields.UNIT_FIELD_POWER_REGEN_FLAT_MODIFIER, 1082423867);
			//if(!ValuesDictionary.ContainsKey((int)EUnitFields.PLAYER_FIELD_MAX_LEVEL)) ValuesDictionary.Add((int)EUnitFields.PLAYER_FIELD_MAX_LEVEL, 60);

			int[] values = SetBitMaskAndBuildValues(bitMaskPlayer, valuesDictionary);

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

		private static void InitializeContainerFields(UpdateFieldValueCollection updateCollection, Dictionary<int, int> valuesDictionary, ref int valueIndex)
		{
			for(int i = (int)EItemFields_Vanilla.ITEM_END; i < updateCollection.UpdateMask.Length && i < (int)EContainerFields_Vanilla.CONTAINER_END; i++)
			{
				bool shouldWrite = VanillaToWotlkConverter.ConvertUpdateFieldsContainer((EContainerFields_Vanilla)i, out int shiftAmount);

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
							throw new InvalidOperationException($"Failed to insert: i:{i}:{((EContainerFields_Vanilla)i).ToString()} [{i + shiftAmount}] [{((EContainerFields_Vanilla)(i + shiftAmount)).ToString()}] {updateCollection.UpdateDiffValues.Reinterpret<int>(valueIndex * sizeof(int))} into dictionary. \n\n Exception: {e.Message}", e);
						}
					}

					//no matter what the value index should increase. Because
					//otherwise it will get descyned from the new values
					valueIndex++;
				}
			}
		}

		private static void InitializeGameObjectFields(UpdateFieldValueCollection updateCollection, Dictionary<int, int> valuesDictionary, ref int valueIndex)
		{
			for(int i = (int)EObjectFields.OBJECT_END; i < updateCollection.UpdateMask.Length && i < (int)EGameObjectFields_Vanilla.OBJECT_END; i++)
			{
				bool shouldWrite = VanillaToWotlkConverter.ConvertUpdateFieldsGameObject((EGameObjectFields_Vanilla)i, out int shiftAmount);

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
							throw new InvalidOperationException($"Failed to insert: i:{i}:{((EGameObjectFields_Vanilla)i).ToString()} [{i + shiftAmount}] [{((EGameObjectFields_Vanilla)(i + shiftAmount)).ToString()}] {updateCollection.UpdateDiffValues.Reinterpret<int>(valueIndex * sizeof(int))} into dictionary. \n\n Exception: {e.Message}", e);
						}
					}

					//no matter what the value index should increase. Because
					//otherwise it will get descyned from the new values
					valueIndex++;
				}
			}
		}

		private static void InitializeItemFields(UpdateFieldValueCollection updateCollection, Dictionary<int, int> valuesDictionary, ref int valueIndex)
		{
			for(int i = (int)EObjectFields.OBJECT_END; i < updateCollection.UpdateMask.Length && i < (int)EItemFields_Vanilla.ITEM_END; i++)
			{
				bool shouldWrite = VanillaToWotlkConverter.ConvertUpdateFieldsItem((EItemFields_Vanilla)i, out int shiftAmount);

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
							throw new InvalidOperationException($"Failed to insert: i:{i}:{((EItemFields_Vanilla)i).ToString()} [{i + shiftAmount}] [{((EItemFields_Vanilla)(i + shiftAmount)).ToString()}] {updateCollection.UpdateDiffValues.Reinterpret<int>(valueIndex * sizeof(int))} into dictionary. \n\n Exception: {e.Message}", e);
						}
					}

					//no matter what the value index should increase. Because
					//otherwise it will get descyned from the new values
					valueIndex++;
				}
			}
		}

		private static void InitializeObjectFields(UpdateFieldValueCollection updateCollection, Dictionary<int, int> valuesDictionary, ref int valueIndex)
		{
			//Wotlk and vanilla both have the same object field
			for(int i = 0; i < (int)EObjectFields.OBJECT_END; i++)
			{
				if(updateCollection.UpdateMask[i])
				{
					valuesDictionary.Add(i, updateCollection.UpdateDiffValues.Reinterpret<int>(valueIndex * sizeof(int)));
					valueIndex++;
				}
			}
		}

		private static int[] SetBitMaskAndBuildValues(BitArray bitMaskPlayer, Dictionary<int, int> valuesDictionary)
		{
			int valueArrayIndex = 0;
			int[] values = new int[valuesDictionary.Count];
			//We have to build the bitmask now
			foreach(var kvp in valuesDictionary.OrderBy(k => k.Key))
			{
				bitMaskPlayer.Set(kvp.Key, true);
				values[valueArrayIndex] = kvp.Value;
				valueArrayIndex++;
			}

			//TODO: Remove this debug stuff
			if(valuesDictionary.ContainsKey((int)EObjectFields.OBJECT_FIELD_GUID))
				File.WriteAllLines($"{valuesDictionary[(int)EObjectFields.OBJECT_FIELD_GUID]}_GUID_" + Guid.NewGuid(), valuesDictionary.OrderBy(kvp => kvp.Key).Select(kvp => $"[{kvp.Key}]:[0x{kvp.Key:X}] [{(EUnitFields)kvp.Key}] Value: {kvp.Value}/{kvp.Value.Reinterpret().Reinterpret<float>()}"));

			return values;
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
	}
}
