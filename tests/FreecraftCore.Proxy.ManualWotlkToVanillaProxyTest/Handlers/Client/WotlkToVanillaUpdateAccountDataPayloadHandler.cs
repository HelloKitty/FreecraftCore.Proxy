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
	[ClientPayloadHandler]
	public sealed class WotlkToVanillaUpdateAccountDataPayloadHandler : BaseGameClientPayloadHandler<CMSG_UPDATE_ACCOUNT_DATA_PAYLOAD>
	{
		/// <inheritdoc />
		public WotlkToVanillaUpdateAccountDataPayloadHandler([NotNull] ILog logger)
			: base(logger)
		{
			
		}

		/// <inheritdoc />
		public override async Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, CMSG_UPDATE_ACCOUNT_DATA_PAYLOAD payload)
		{
			//So vanilla cmangos and mangos servers do not actually handle this packet
			//They just discard it. So we will send this packet, though it won't do anything,
			//and then we will respond to the client that the update was complete.

			//Forward and then respond to client, vanilla server won't respond.
			await context.ProxyConnection.SendMessage(payload);
			await context.PayloadSendService.SendMessage(new SMSG_UPDATE_ACCOUNT_DATA_COMPLETE_PAYLOAD(payload.DataType));
		}
	}
}
