using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

			//TODO: Renable object update
			GamePacketMetadataMarker
				.SerializableTypes
				.Where(p => !p.Name.ToUpper().Contains("ADDON"))
				.ToList()
				.ForEach(t => serializer.RegisterType(t));

			//This will register dynamic DTO that have a byte[] data field allowing unknown packets to be sent
			//over the network to client/server. FreecraftCore.Serializer does not support serializing default types
			//so this is the work around
			//Registeration can be slow.
			//foreach(Type t in GamePacketMetadataMarker.GamePacketPayloadTypesWithDynamicProxies.Value)
			//	serializer.RegisterType(t);
			foreach(Type t in GamePacketStubMetadataMarker.GamePacketPayloadStubTypes.Where(s => GamePacketMetadataMarker.UnimplementedOperationCodes.Value.Contains(s.GetCustomAttribute<GamePayloadOperationCodeAttribute>().OperationCode))/*.Concat(new Type[2] { typeof(SMSG_COMPRESSED_UPDATE_OBJECT_DTO_PROXY), typeof(SMSG_UPDATE_OBJECT_DTO_PROXY)})*/)
				serializer.RegisterType(t);

			//Manually register PROXY_DTO
			//serializer.RegisterType(typeof(SMSG_COMPRESSED_UPDATE_OBJECT_DTO_PROXY));
			//serializer.RegisterType(typeof(SMSG_UPDATE_OBJECT_DTO_PROXY));
			serializer.RegisterType<SMSG_ADDON_INFO_DTO_PROXY>();

			//Also the header types
			serializer.RegisterType<ServerPacketHeader>();
			serializer.RegisterType<OutgoingClientPacketHeader>();

			serializer.Compile();

			return new FreecraftCoreGladNetSerializerAdapter(serializer);
		}
	}
}
