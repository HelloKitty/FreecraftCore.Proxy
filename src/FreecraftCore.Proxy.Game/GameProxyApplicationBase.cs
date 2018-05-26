using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common.Logging;
using FreecraftCore.Packet;
using FreecraftCore.Packet.Auth;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public class GameProxyApplicationBase : ProxiedTcpServerApplicationBase<GamePacketPayload, GamePacketPayload>
	{
		/// <inheritdoc />
		protected GameProxyApplicationBase(NetworkAddressInfo serverAddress, [NotNull] ILog logger, PayloadHandlerRegisterationModules<GamePacketPayload, GamePacketPayload> handlerModulePair, NetworkSerializerServicePair serializers) 
			: base(serverAddress, logger, handlerModulePair, serializers)
		{

		}

		/// <inheritdoc />
		protected override IManagedNetworkServerClient<GamePacketPayload, GamePacketPayload> BuildIncomingSessionManagedClient(NetworkClientBase clientBase, INetworkSerializationService serializeService)
		{
			//TODO: Are any details actually valuable here?
			if(Logger.IsInfoEnabled)
				Logger.Info($"Client connected to proxy.");

			//The auth server is encryptionless and 'headerless' so we do not need to support that on the proxy for the auth server
			return clientBase
				.AddHeaderReading<IncomingClientPacketHeader>(serializeService)
				.AddHeaderlessNetworkMessageReading(serializeService)
				.For<GamePacketPayload, GamePacketPayload, IGamePacketPayload>()
				.Build()
				.AsManagedSession(Logger);
		}

		/// <inheritdoc />
		protected override IManagedNetworkClient<GamePacketPayload, GamePacketPayload> BuildOutgoingSessionManagedClient(NetworkClientBase clientBase, INetworkSerializationService serializeService)
		{
			//We need to create an actual client to the server too.
			return clientBase
				.AddHeaderlessNetworkMessageReading(serializeService)
				.For<GamePacketPayload, GamePacketPayload, IGamePacketPayload>()
				.Build()
				.AsManagedSession(Logger);
		}

		/// <inheritdoc />
		protected override ContainerBuilder RegisterHandlerDependencies(ContainerBuilder builder)
		{
			//The default handlers (Just forwards)
			builder.RegisterType<GameDefaultServerResponseHandler>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterType<GameDefaultClientRequestHandler>()
				.AsImplementedInterfaces()
				.SingleInstance();

			return builder;
		}
	}
}
