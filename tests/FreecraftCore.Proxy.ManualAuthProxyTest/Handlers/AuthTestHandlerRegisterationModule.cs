using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class AuthTestHandlerRegisterationModule : PayloadHandlerRegisterationModules<AuthenticationClientPayload, AuthenticationServerPayload>
	{
		/// <inheritdoc />
		public AuthTestHandlerRegisterationModule() 
			: base(new List<PayloadHandlerRegisterationModule<AuthenticationClientPayload, AuthenticationServerPayload, IProxiedMessageContext<AuthenticationServerPayload, AuthenticationClientPayload>>>(), new List<PayloadHandlerRegisterationModule<AuthenticationServerPayload, AuthenticationClientPayload, IProxiedMessageContext<AuthenticationClientPayload, AuthenticationServerPayload>>>())
		{

		}

		public void AddServerHandlerModule([NotNull] PayloadHandlerRegisterationModule<AuthenticationServerPayload, AuthenticationClientPayload, IProxiedMessageContext<AuthenticationClientPayload, AuthenticationServerPayload>> handlerModule)
		{
			if(handlerModule == null) throw new ArgumentNullException(nameof(handlerModule));

			var list = this.ServerMessageHandlerModules as List<PayloadHandlerRegisterationModule<AuthenticationServerPayload, AuthenticationClientPayload, IProxiedMessageContext<AuthenticationClientPayload, AuthenticationServerPayload>>>;

			list.Add(handlerModule);
		}

		public void AddClientHanderModule([NotNull] PayloadHandlerRegisterationModule<AuthenticationClientPayload, AuthenticationServerPayload, IProxiedMessageContext<AuthenticationServerPayload, AuthenticationClientPayload>> handlerModule)
		{
			if(handlerModule == null) throw new ArgumentNullException(nameof(handlerModule));

			var list = this.ClientMessageHandlerModules as List<PayloadHandlerRegisterationModule<AuthenticationClientPayload, AuthenticationServerPayload, IProxiedMessageContext<AuthenticationServerPayload, AuthenticationClientPayload>>>;

			list.Add(handlerModule);
		}
	}
}
