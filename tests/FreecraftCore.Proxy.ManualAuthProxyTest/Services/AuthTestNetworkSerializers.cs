using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreecraftCore.Packet.Auth;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class AuthTestNetworkSerializers : NetworkSerializerServicePair
	{
		/// <inheritdoc />
		public AuthTestNetworkSerializers() 
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

			Type[] AuthPayloads = new Type[] { typeof(AuthLogonChallengeRequest), typeof(AuthLogonChallengeResponse), typeof(AuthLogonProofResponse), typeof(AuthLogonProofRequest), typeof(AuthRealmListRequest), typeof(AuthRealmListResponse) };

			AuthPayloads
				.ToList()
				.ForEach(t => serializer.RegisterType(t));

			serializer.Compile();

			return new FreecraftCoreGladNetSerializerAdapter(serializer);
		}
	}
}
