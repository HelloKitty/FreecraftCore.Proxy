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
	/// <summary>
	/// Simplied type alias for authentication handlers that handle Server payloads sent from the server.
	/// </summary>
	public abstract class BaseAuthenticationServerPayloadHandler<TSpecificPayloadType> : IPeerPayloadSpecificMessageHandler<TSpecificPayloadType, AuthenticationClientPayload, IProxiedMessageContext<AuthenticationClientPayload, AuthenticationServerPayload>>
		where TSpecificPayloadType : AuthenticationServerPayload
	{
		/// <summary>
		/// The logger for the handler.
		/// </summary>
		protected ILog Logger { get; }

		/// <inheritdoc />
		protected BaseAuthenticationServerPayloadHandler([NotNull] ILog logger)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public abstract Task HandleMessage(IProxiedMessageContext<AuthenticationClientPayload, AuthenticationServerPayload> context, TSpecificPayloadType payload);
	}
}
