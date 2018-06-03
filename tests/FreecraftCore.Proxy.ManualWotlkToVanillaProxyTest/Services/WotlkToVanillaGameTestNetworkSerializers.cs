using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Fasterflect;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;
using Org.BouncyCastle.Utilities.Collections;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaGameTestNetworkSerializers : NetworkSerializerServicePair
	{
		/// <inheritdoc />
		public WotlkToVanillaGameTestNetworkSerializers()
			: base(BuildClientSerializer(), BuildServerSerializer())
		{

		}

		//TODO: We should use seperate assemblies that can build the desired serializers
		private static INetworkSerializationService BuildServerSerializer()
		{
			SerializerService serializer = new SerializerService();

			//This is slightly complicated
			//But we need to register all the vanilla DTOs
			//but we should also register all the wotlk DTOs that we don't have vanilla DTOs for
			//This design will change in the future so that there is: wotlk, vanilla and shared libraries
			//But right now this is how we have to do it until then
			IReadOnlyCollection<NetworkOperationCode> codes = VanillaGamePacketMetadataMarker.UnimplementedOperationCodes.Value;
			HashSet<NetworkOperationCode> opcodeSet = new HashSet<NetworkOperationCode>();
			foreach(var opcode in codes)
				opcodeSet.Add(opcode);

			GamePacketStubMetadataMarker
				.GamePacketPayloadStubTypes
				.Where(t =>
				{
					NetworkOperationCode code = t.Attribute<GamePayloadOperationCodeAttribute>().OperationCode;

					//if it's not a vanilla or shared packet
					return opcodeSet.Contains(code) && GamePacketMetadataMarker.UnimplementedOperationCodes.Value.Contains(code);
				})
				.Concat(GamePacketMetadataMarker.GamePacketPayloadTypes.Where(t => opcodeSet.Contains(t.Attribute<GamePayloadOperationCodeAttribute>().OperationCode)))
				.Concat(VanillaGamePacketMetadataMarker.VanillaGamePacketPayloadTypes)
				//TODO: Disable this when you need
				//.Where(t => t != typeof(SMSG_COMPRESSED_UPDATE_OBJECT_Payload_Vanilla) && t != typeof(SMSG_UPDATE_OBJECT_Payload_Vanilla))
				.ToList()
				.ForEach(t =>
				{
					if(!(typeof(IUnimplementedGamePacketPayload).IsAssignableFrom(t)))
						Console.WriteLine($"Registering Type: {t.Name}");

					serializer.RegisterType(t);
				});

			//Also the header types
			serializer.RegisterType<ServerPacketHeader>();
			serializer.RegisterType<OutgoingClientPacketHeader>();

			//TODO: Uncomment for dumping
			//serializer.RegisterType(typeof(SMSG_COMPRESSED_UPDATE_OBJECT_DTO_PROXY));
			//serializer.RegisterType(typeof(SMSG_UPDATE_OBJECT_DTO_PROXY));

			serializer.Compile();

			return new FreecraftCoreGladNetSerializerAdapter(serializer);
		}

		private static INetworkSerializationService BuildClientSerializer()
		{
			SerializerService serializer = new SerializerService();

			GamePacketMetadataMarker
				.SerializableTypes
				.Concat(GamePacketMetadataMarker.GamePacketPayloadTypes)
				.ToList()
				.ForEach(t => serializer.RegisterType(t));

			//Register all unimplemented stubs
			foreach(Type t in GamePacketStubMetadataMarker.GamePacketPayloadStubTypes.Where(t => GamePacketMetadataMarker.UnimplementedOperationCodes.Value.Contains(t.GetCustomAttribute<GamePayloadOperationCodeAttribute>().OperationCode)))
				serializer.RegisterType(t);

			//Also the header types
			serializer.RegisterType<ServerPacketHeader>();
			serializer.RegisterType<OutgoingClientPacketHeader>();

			serializer.Compile();

			return new FreecraftCoreGladNetSerializerAdapter(serializer);
		}
	}
}
