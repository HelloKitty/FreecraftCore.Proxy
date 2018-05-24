using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using FreecraftCore.Packet.Auth;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/*public sealed class AuthenticationLogonChallengeSessionHandler : IPeerPayloadSpecificMessageHandler<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>
	{
		private ILog Logger { get; }

		/// <inheritdoc />
		public AuthenticationLogonChallengeSessionHandler([NotNull] ILog logger)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public Task HandleMessage(ProxiedAuthenticationSessionMessageContext context, AuthenticationClientPayload payload)
		{
			//TODO: For a real implementation we'd want to make potential modifications and then forward the challenge
			Logger.Debug($"Recieved authlogon challenge on the handler for the proxy.");

			return Task.CompletedTask;
		}
	}*/
}
