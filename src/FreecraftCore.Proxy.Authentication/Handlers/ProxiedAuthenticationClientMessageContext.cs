using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.Packet.Auth;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	//TODO: Add service/interface/context that allows server handlers to send messages through the client
	public sealed class ProxiedAuthenticationClientMessageContext : IPeerSessionMessageContext<AuthenticationClientPayload>
	{
		/// <inheritdoc />
		public IConnectionService ConnectionService { get; }

		/// <inheritdoc />
		public IPeerPayloadSendService<AuthenticationClientPayload> PayloadSendService { get; }

		/// <inheritdoc />
		public IPeerRequestSendService<AuthenticationClientPayload> RequestSendService { get; }

		/// <inheritdoc />
		public SessionDetails Details { get; }

		public IManagedNetworkClient<AuthenticationServerPayload, AuthenticationClientPayload> ProxyClient { get; }

		/// <inheritdoc />
		public ProxiedAuthenticationClientMessageContext([NotNull] IConnectionService connectionService, [NotNull] IPeerPayloadSendService<AuthenticationClientPayload> payloadSendService, 
			[NotNull] IPeerRequestSendService<AuthenticationClientPayload> requestSendService, [NotNull] SessionDetails details, [NotNull] IManagedNetworkClient<AuthenticationServerPayload, AuthenticationClientPayload> proxyClient)
		{
			if(connectionService == null) throw new ArgumentNullException(nameof(connectionService));
			if(payloadSendService == null) throw new ArgumentNullException(nameof(payloadSendService));
			if(requestSendService == null) throw new ArgumentNullException(nameof(requestSendService));
			if(details == null) throw new ArgumentNullException(nameof(details));

			ConnectionService = connectionService;
			PayloadSendService = payloadSendService;
			RequestSendService = requestSendService;
			Details = details;
			ProxyClient = proxyClient ?? throw new ArgumentNullException(nameof(proxyClient));
		}
	}
}
