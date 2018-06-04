using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// Simple base handler that provides an API for converting the incoming payload
	/// to the outgoing payload type.
	/// (Ex. from Vanilla WoW to Wotlk packets).
	/// </summary>
	/// <typeparam name="TPayloadInputType">The input payload type. (The Type to capture)</typeparam>
	/// <typeparam name="TPayloadOutputType">The output payload type. (The Type to send to the proxied connection)</typeparam>
	public abstract class GameClientPayloadConverterHandler<TPayloadInputType, TPayloadOutputType> : BaseGameClientPayloadHandler<TPayloadInputType>
		where TPayloadInputType : GamePacketPayload
		where TPayloadOutputType : GamePacketPayload
	{
		/// <inheritdoc />
		protected GameClientPayloadConverterHandler([NotNull] ILog logger)
			: base(logger)
		{
		}

		/// <inheritdoc />
		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, TPayloadInputType payload)
		{
			//TODO: Should we handle null as a way to indicate don't send?
			return context.ProxyConnection.SendMessage(ConvertToOutputPayload(payload));
		}

		/// <summary>
		/// Converter method used to convert the <typeparamref name="TPayloadInputType"/> to
		/// the <see cref="TPayloadOutputType"/>.
		/// </summary>
		/// <param name="payload">The recieved payload.</param>
		/// <returns>A non-null output payload.</returns>
		protected abstract TPayloadOutputType ConvertToOutputPayload(TPayloadInputType payload);
	}
}
