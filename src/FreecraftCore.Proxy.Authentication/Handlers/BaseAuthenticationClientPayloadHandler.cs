using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.Packet.Auth;
using GladNet;

namespace FreecraftCore
{
	/// <summary>
	/// Simplied type alias for authentication handlers that handle Client payloads sent from the client.
	/// </summary>
	[ClientPayloadHandler]
	public abstract class BaseAuthenticationClientPayloadHandler<TSpecificPayloadType> : IPeerPayloadSpecificMessageHandler<TSpecificPayloadType, AuthenticationServerPayload, IProxiedMessageContext<AuthenticationServerPayload, AuthenticationClientPayload>>
		where TSpecificPayloadType : AuthenticationClientPayload
	{
		/// <inheritdoc />
		public abstract Task HandleMessage(IProxiedMessageContext<AuthenticationServerPayload, AuthenticationClientPayload> context, TSpecificPayloadType payload);
	}
}
