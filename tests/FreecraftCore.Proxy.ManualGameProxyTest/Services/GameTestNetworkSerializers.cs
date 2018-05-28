﻿using System;
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
		/// <inheritdoc />
		public GameTestNetworkSerializers()
			: base(BuildClientSerializer(), BuildServerSerializer())
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

			//Also the header types
			serializer.RegisterType<ServerPacketHeader>();
			serializer.RegisterType<OutgoingClientPacketHeader>();

			serializer.Compile();

			return new FreecraftCoreGladNetSerializerAdapter(serializer);
		}
	}
}
