using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// Simplied type alias for game handlers that handle Client payloads sent from the client.
	/// </summary>
	[ClientPayloadHandler]
	public abstract class BaseGameClientPayloadHandler<TSpecificPayloadType> : BaseGamePayloadHandler<TSpecificPayloadType>
		where TSpecificPayloadType : GamePacketPayload
	{
		/// <inheritdoc />
		protected BaseGameClientPayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{

		}
	}
}
