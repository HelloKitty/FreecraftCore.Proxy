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
	public sealed class ProxiedAuthenticationConnectionSession : ProxiedManagedClientSession<AuthenticationServerPayload, AuthenticationClientPayload, ProxiedAuthenticationSessionMessageContext>
	{
		/// <inheritdoc />
		public ProxiedAuthenticationConnectionSession(IManagedNetworkServerClient<AuthenticationServerPayload, AuthenticationClientPayload> internalManagedNetworkClient, 
			SessionDetails details, 
			[NotNull] MessageHandlerService<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext> authMessageHandlerService, 
			IGenericMessageContextFactory<AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext> messageContextFactory) 
			: base(internalManagedNetworkClient, details, authMessageHandlerService, messageContextFactory)
		{

		}
	}
}
