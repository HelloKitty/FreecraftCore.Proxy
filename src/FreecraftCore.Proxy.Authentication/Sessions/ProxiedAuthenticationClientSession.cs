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
	public sealed class ProxiedAuthenticationClientSession : ProxiedManagedClientSession<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationClientMessageContext>
	{
		/// <inheritdoc />
		public ProxiedAuthenticationClientSession(IManagedNetworkServerClient<AuthenticationClientPayload, AuthenticationServerPayload> internalManagedNetworkClient, SessionDetails details, [NotNull] MessageHandlerService<AuthenticationServerPayload, AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext> authMessageHandlerService, IGenericMessageContextFactory<AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext> messageContextFactory) 
			: base(internalManagedNetworkClient, details, authMessageHandlerService, messageContextFactory)
		{

		}
	}
}
