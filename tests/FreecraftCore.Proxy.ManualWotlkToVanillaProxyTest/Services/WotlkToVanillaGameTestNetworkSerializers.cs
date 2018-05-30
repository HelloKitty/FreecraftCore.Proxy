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

			GamePacketMetadataMarker
				.GamePacketPayloadTypes
				.Concat(GamePacketStubMetadataMarker.GamePacketPayloadStubTypes)
				.Where(t => opcodeSet.Contains(t.Attribute<GamePayloadOperationCodeAttribute>().OperationCode))
				.Concat(VanillaGamePacketMetadataMarker.VanillaGamePacketPayloadTypes)
				.ToList()
				.ForEach(t => serializer.RegisterType(t));

			//Also the header types
			serializer.RegisterType<ServerPacketHeader>();
			serializer.RegisterType<OutgoingClientPacketHeader>();

			serializer.Compile();

			return new FreecraftCoreGladNetSerializerAdapter(serializer);
		}

		private static INetworkSerializationService BuildClientSerializer()
		{
			SerializerService serializer = new SerializerService();

			GamePacketMetadataMarker
				.SerializableTypes
				.ToList()
				.ForEach(t => serializer.RegisterType(t));

			//This will register dynamic DTO that have a byte[] data field allowing unknown packets to be sent
			//over the network to client/server. FreecraftCore.Serializer does not support serializing default types
			//so this is the work around
			//Registeration can be slow.
			//foreach(Type t in GamePacketMetadataMarker.GamePacketPayloadTypesWithDynamicProxies.Value)
			//	serializer.RegisterType(t);
			foreach(Type t in GamePacketStubMetadataMarker.GamePacketPayloadStubTypes)
				serializer.RegisterType(t);

			//Also the header types
			serializer.RegisterType<ServerPacketHeader>();
			serializer.RegisterType<OutgoingClientPacketHeader>();

			serializer.Compile();

			return new FreecraftCoreGladNetSerializerAdapter(serializer);
		}
	}
}
