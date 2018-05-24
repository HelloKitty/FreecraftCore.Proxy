using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.API.Common;
using FreecraftCore.Packet.Auth;
using GladNet;
using JetBrains.Annotations;
using Moq;

namespace FreecraftCore
{
	public sealed class ProxiedAuthenticationConnectionSession : ManagedClientSession<AuthenticationServerPayload, AuthenticationClientPayload>
	{
		/// <summary>
		/// The message handling service for auth messages.
		/// </summary>
		private MessageHandlerService<AuthenticationClientPayload, AuthenticationServerPayload, IPeerSessionMessageContext<AuthenticationServerPayload>> AuthMessageHandlerService { get; }

		public static IPeerRequestSendService<AuthenticationServerPayload> MockedInterceptor { get; }

		static ProxiedAuthenticationConnectionSession()
		{
			//TODO: Implement when design issue in GladNet3 is fixed we can stop mocking this dependency.
			MockedInterceptor = Mock.Of<IPeerRequestSendService<AuthenticationServerPayload>>();
		}

		/// <inheritdoc />
		public ProxiedAuthenticationConnectionSession(IManagedNetworkServerClient<AuthenticationServerPayload, AuthenticationClientPayload> internalManagedNetworkClient, SessionDetails details,
			[NotNull] MessageHandlerService<AuthenticationClientPayload, AuthenticationServerPayload, IPeerSessionMessageContext<AuthenticationServerPayload>> authMessageHandlerService)
			: base(internalManagedNetworkClient, details)
		{
			if(authMessageHandlerService == null) throw new ArgumentNullException(nameof(authMessageHandlerService));

			AuthMessageHandlerService = authMessageHandlerService;
		}

		/// <inheritdoc />
		public override Task OnNetworkMessageRecieved(NetworkIncomingMessage<AuthenticationClientPayload> message)
		{
			//TODO: How should we handle server not having interceptor
			return AuthMessageHandlerService.TryHandleMessage(new AuthSessionMessageContext(Connection, SendService, MockedInterceptor, Details), message);
		}

		/// <inheritdoc />
		protected override void OnSessionDisconnected()
		{
			//TODO: If the authserver disconnects us we should disconnect the proxied client too in the same way
		}
	}
}
