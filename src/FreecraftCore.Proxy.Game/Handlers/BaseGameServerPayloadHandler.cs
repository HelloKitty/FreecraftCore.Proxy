using System;
using System.Threading.Tasks;
using Common.Logging;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// Simplied type alias for game handlers that handle Server payloads sent from the server.
	/// </summary>
	[ServerPayloadHandler]
	public abstract class BaseGameServerPayloadHandler<TSpecificPayloadType> : BaseGamePayloadHandler<TSpecificPayloadType>
		where TSpecificPayloadType : GamePacketPayload
	{
		/// <inheritdoc />
		protected BaseGameServerPayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{

		}
	}
}