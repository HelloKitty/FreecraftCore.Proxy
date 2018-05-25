using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using FreecraftCore.Packet.Auth;
using GladNet;
using JetBrains.Annotations;
using Moq;

namespace FreecraftCore
{
	public sealed class ProxiedClientMessageContextFactory : IGenericMessageContextFactory<AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext>
	{
		public static IPeerRequestSendService<AuthenticationClientPayload> Interceptor = Mock.Of<IPeerRequestSendService<AuthenticationClientPayload>>();

		private IManagedNetworkClient<AuthenticationServerPayload, AuthenticationClientPayload> ProxyClient { get; }

		/// <inheritdoc />
		public ProxiedClientMessageContextFactory([NotNull] IManagedNetworkClient<AuthenticationServerPayload, AuthenticationClientPayload> proxyClient)
		{
			ProxyClient = proxyClient ?? throw new ArgumentNullException(nameof(proxyClient));
		}

		/// <inheritdoc />
		public ProxiedAuthenticationClientMessageContext CreateMessageContext(IConnectionService connectionService, IPeerPayloadSendService<AuthenticationClientPayload> sendService, SessionDetails details)
		{
			return new ProxiedAuthenticationClientMessageContext(connectionService, sendService, Interceptor, details, ProxyClient);
		}
	}

	public sealed class ProxiedSessionMessageContextFactory : IGenericMessageContextFactory<AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>
	{
		public static IPeerRequestSendService<AuthenticationServerPayload> Interceptor = Mock.Of<IPeerRequestSendService<AuthenticationServerPayload>>();

		private IManagedNetworkClient<AuthenticationClientPayload, AuthenticationServerPayload> ProxyClient { get; }

		/// <inheritdoc />
		public ProxiedSessionMessageContextFactory([NotNull] IManagedNetworkClient<AuthenticationClientPayload, AuthenticationServerPayload> proxyClient)
		{
			ProxyClient = proxyClient ?? throw new ArgumentNullException(nameof(proxyClient));
		}

		/// <inheritdoc />
		public ProxiedAuthenticationSessionMessageContext CreateMessageContext(IConnectionService connectionService, IPeerPayloadSendService<AuthenticationServerPayload> sendService, SessionDetails details)
		{
			return new ProxiedAuthenticationSessionMessageContext(connectionService, sendService, Interceptor, details, ProxyClient);
		}
	}

	public sealed class ProxiedMessageContextFactory
	{
		public IManagedNetworkClient<AuthenticationClientPayload, AuthenticationServerPayload> ProxyClientClient { get; internal set; }

		public IManagedNetworkClient<AuthenticationServerPayload, AuthenticationClientPayload> ProxyClientServer { get; internal set; }

		private SessionDetails SessionDetails { get; } = new SessionDetails(new NetworkAddressInfo(IPAddress.IPv6Loopback, 3724), 1);

		public ProxiedAuthenticationSessionMessageContext CreateSessionMessageContext(IConnectionService connectionService, IPeerPayloadSendService<AuthenticationServerPayload> sendService, IPeerRequestSendService<AuthenticationServerPayload> requestService) 
		{
			if(ProxyClientServer == null)
				throw new InvalidOperationException("Cannot proxy messsage without valid proxy client running.");

			return new ProxiedAuthenticationSessionMessageContext(connectionService, sendService, requestService, SessionDetails, ProxyClientClient);
		}

		public ProxiedAuthenticationClientMessageContext CreateClientMessageContext(IConnectionService connectionService, IPeerPayloadSendService<AuthenticationClientPayload> sendService, IPeerRequestSendService<AuthenticationClientPayload> requestService)
		{
			if(ProxyClientServer == null)
				throw new InvalidOperationException("Cannot proxy messsage without valid proxy client running.");

			return new ProxiedAuthenticationClientMessageContext(connectionService, sendService, requestService, SessionDetails, ProxyClientServer);
		}
	}
}
