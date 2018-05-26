using System;
using System.Collections.Generic;
using System.Text;
using FreecraftCore.Packet.Auth;

namespace FreecraftCore
{
	/// <summary>
	/// Simplied Type alias for authentication registeration module for client handlers.
	/// </summary>
	public abstract class AuthenticationClientPayloadHandlerRegisterationModule : PayloadHandlerRegisterationModule<AuthenticationClientPayload, AuthenticationServerPayload, IProxiedMessageContext<AuthenticationServerPayload, AuthenticationClientPayload>>
	{
		
	}
}
