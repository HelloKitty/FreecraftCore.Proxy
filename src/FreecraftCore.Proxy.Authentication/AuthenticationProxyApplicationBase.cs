using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common.Logging;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public class AuthenticationProxyApplicationBase : ProxiedTcpServerApplicationBase<AuthenticationServerPayload, AuthenticationClientPayload>
	{
		/// <inheritdoc />
		public AuthenticationProxyApplicationBase(NetworkAddressInfo listenerAddress, [NotNull] NetworkAddressInfo proxyToEndpointAddress, [NotNull] ILog logger, PayloadHandlerRegisterationModules<AuthenticationClientPayload, AuthenticationServerPayload> handlerModulePair, NetworkSerializerServicePair serializers) 
			: base(listenerAddress, proxyToEndpointAddress, logger, handlerModulePair, serializers)
		{

		}

		/// <inheritdoc />
		protected override IManagedNetworkServerClient<AuthenticationServerPayload, AuthenticationClientPayload> BuildIncomingSessionManagedClient(NetworkClientBase clientBase, INetworkSerializationService serializeService)
		{
			//TODO: Are any details actually valuable here?
			if(Logger.IsInfoEnabled)
				Logger.Info($"Client connected to proxy.");

			//The auth server is encryptionless and 'headerless' so we do not need to support that on the proxy for the auth server
			return clientBase
				.AddHeaderlessNetworkMessageReading(serializeService)
				.For<AuthenticationClientPayload, AuthenticationServerPayload, IAuthenticationPayload>()
				.Build()
				.AsManagedSession(Logger);
		}

		/// <inheritdoc />
		protected override IManagedNetworkClient<AuthenticationClientPayload, AuthenticationServerPayload> BuildOutgoingSessionManagedClient(NetworkClientBase clientBase, INetworkSerializationService serializeService)
		{
			//We need to create an actual client to the server too.
			return clientBase
				.AddHeaderlessNetworkMessageReading(serializeService)
				.For<AuthenticationServerPayload, AuthenticationClientPayload, IAuthenticationPayload>()
				.Build()
				.AsManagedSession(Logger);
		}

		/// <inheritdoc />
		protected override ContainerBuilder RegisterHandlerDependencies(ContainerBuilder builder)
		{
			return builder;
		}

		/// <inheritdoc />
		protected override ContainerBuilder RegisterDefaultHandlers(ContainerBuilder builder)
		{
			//The default handlers (Just forwards)
			builder.RegisterType<AuthDefaultServerResponseHandler>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterType<AuthDefaultClientRequestHandler>()
				.AsImplementedInterfaces()
				.SingleInstance();

			return builder;
		}
	}
}
