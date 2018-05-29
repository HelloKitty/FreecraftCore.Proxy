using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class GameTestNetworkSerializers : NetworkSerializerServicePair
	{
		private static INetworkSerializationService Serializer { get; } = BuildClientSerializer();

		/// <inheritdoc />
		public GameTestNetworkSerializers()
			: base(Serializer, Serializer)
		{

		}

		//TODO: We should use seperate assemblies that can build the desired serializers
		private static INetworkSerializationService BuildServerSerializer()
		{
			return BuildClientSerializer();
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
