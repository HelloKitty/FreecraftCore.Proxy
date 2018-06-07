using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaPlayedTimeClientPayloadHandler : BaseGameClientPayloadHandler<CMSG_PLAYED_TIME_DTO_PROXY>
	{
		/// <inheritdoc />
		public WotlkToVanillaPlayedTimeClientPayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{
		}

		/// <inheritdoc />
		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, CMSG_PLAYED_TIME_DTO_PROXY payload)
		{
			//TODO: Is this required? Does Mangos care if there is a byte here from 3.3.5?
			return context.ProxyConnection.SendMessage(new CMSG_PLAYED_TIME_DTO_PROXY() {Data = Array.Empty<byte>()});
		}
	}
}
