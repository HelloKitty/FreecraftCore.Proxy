using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.Packet.Auth;
using GladNet;

namespace FreecraftCore
{
	public sealed class AuthenticationRealmListResponseClientHandler : IPeerPayloadSpecificMessageHandler<AuthRealmListResponse, AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext>
	{
		/// <inheritdoc />
		public Task HandleMessage(ProxiedAuthenticationClientMessageContext context, AuthRealmListResponse payload)
		{
			//We have to rebuild the realm list to point to the proxy AND to adjust client build number

			return Task.CompletedTask;
		}
	}
}
