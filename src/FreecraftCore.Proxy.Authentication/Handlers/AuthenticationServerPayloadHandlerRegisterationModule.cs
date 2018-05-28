using System;
using System.Collections.Generic;
using System.Text;

namespace FreecraftCore
{
	/// <summary>
	/// Simplied Type alias for authentication registeration module for server handlers.
	/// </summary>
	public abstract class AuthenticationServerPayloadHandlerRegisterationModule : PayloadHandlerRegisterationModule<AuthenticationServerPayload, AuthenticationClientPayload, IProxiedMessageContext<AuthenticationClientPayload, AuthenticationServerPayload>>
	{
		
	}
}
