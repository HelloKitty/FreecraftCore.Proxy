using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using FreecraftCore.Packet.Auth;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	[ServerPayloadHandler]
	public sealed class AuthDefaultServerResponseHandler : IPeerPayloadSpecificMessageHandler<AuthenticationServerPayload, AuthenticationClientPayload, IProxiedMessageContext<AuthenticationClientPayload, AuthenticationServerPayload>>
	{
		private ILog Logger { get; }

		/// <inheritdoc />
		public AuthDefaultServerResponseHandler([NotNull] ILog logger)
		{
			if(logger == null) throw new ArgumentNullException(nameof(logger));

			Logger = logger;
		}

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
		/// <inheritdoc />
		public async Task HandleMessage(IProxiedMessageContext<AuthenticationClientPayload, AuthenticationServerPayload> context, AuthenticationServerPayload payload)
		{
			if(Logger.IsWarnEnabled)
				Logger.Warn($"Recieved unproxied Payload: {payload.GetType().Name} on {this.GetType().Name}");

			//TODO: We cannot implement the default behavior of the proxy because some information is lost when we recieve an unknown payload.
			//The information about the opcode is not exposed to the handler so we can just forward unknown messages.
			//Alternatives is to add a middleware/pipeline extension that forwards "uninteresting" opcodes without even
			//handling them.

			//TODO: Check if it is default payload. We don't want to forward defaults/unknowns
			//Forward to the server
			await context.ProxyConnection.SendMessage(payload)
				.ConfigureAwait(false);
		}
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage
	}
}
