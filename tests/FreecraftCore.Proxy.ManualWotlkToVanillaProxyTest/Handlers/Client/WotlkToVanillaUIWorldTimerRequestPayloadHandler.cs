using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binarysharp.MemoryManagement;
using Common.Logging;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// Wotlk client sends this to update the UI time.
	/// So we need to spoof the current unix time.
	/// </summary>
	[ClientPayloadHandler]
	public sealed class WotlkToVanillaUIWorldTimerRequestPayloadHandler : BaseGameClientPayloadHandler<CMSG_WORLD_STATE_UI_TIMER_UPDATE_DTO_PROXY>
	{
		/// <inheritdoc />
		public WotlkToVanillaUIWorldTimerRequestPayloadHandler([NotNull] ILog logger)
			: base(logger)
		{
			
		}

		/// <inheritdoc />
		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, CMSG_WORLD_STATE_UI_TIMER_UPDATE_DTO_PROXY payload)
		{
			//TODO: Is this time correct?
			//Vanilla 1.12.1 does not implement this packet. So we should sent current unix time stamp
			//in the spoofed response.
			return context.PayloadSendService.SendMessage(new SMSG_WORLD_STATE_UI_TIMER_UPDATE_Payload((uint)DateTimeOffset.Now.ToUnixTimeSeconds()));
		}
	}
}
