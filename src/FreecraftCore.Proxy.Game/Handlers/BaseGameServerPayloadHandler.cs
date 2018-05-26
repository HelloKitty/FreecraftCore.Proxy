using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using FreecraftCore.Packet;
using FreecraftCore.Packet.Auth;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// Simplied type alias for game handlers that handle Server payloads sent from the server.
	/// </summary>
	[ServerPayloadHandler]
	public abstract class BaseGameServerPayloadHandler<TSpecificPayloadType> : IPeerPayloadSpecificMessageHandler<TSpecificPayloadType, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>
		where TSpecificPayloadType : GamePacketPayload
	{
		/// <summary>
		/// The logger for the handler.
		/// </summary>
		protected ILog Logger { get; }

		/// <inheritdoc />
		protected BaseGameServerPayloadHandler([NotNull] ILog logger)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public abstract Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, TSpecificPayloadType payload);
	}
}
