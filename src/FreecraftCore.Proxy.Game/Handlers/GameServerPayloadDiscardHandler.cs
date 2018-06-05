using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// Base handler for simplfied discarding logic.
	/// Will just not forward the specified payload.
	/// </summary>
	/// <typeparam name="TPacketToDiscardType"></typeparam>
	public abstract class GameServerPayloadDiscardHandler<TPacketToDiscardType> : BaseGameServerPayloadHandler<TPacketToDiscardType> 
		where TPacketToDiscardType : GamePacketPayload
	{
		/// <inheritdoc />
		protected GameServerPayloadDiscardHandler([NotNull] ILog logger) 
			: base(logger)
		{

		}

		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, TPacketToDiscardType payload)
		{
			if(Logger.IsDebugEnabled)
				Logger.Debug($"Discarded: {payload.GetOperationCode()}");

			return Task.CompletedTask;
		}
	}
}
