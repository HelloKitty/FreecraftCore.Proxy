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
	public sealed class WotlkToVanillaRealmSplitPayloadHandler : BaseGameClientPayloadHandler<RealmSplitRequest>
	{
		/// <inheritdoc />
		public WotlkToVanillaRealmSplitPayloadHandler([NotNull] ILog logger)
			: base(logger)
		{
			
		}

		/// <inheritdoc />
		public override Task HandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, RealmSplitRequest payload)
		{
			//So vanilla servers don't do this packet
			//So we will respond with what Trinitycore sends which also just barley does handling on it

			//Don't use proxy connection because we want to send back to the client
			return context.PayloadSendService.SendMessage(new RealmSplitResponse(payload.Unk));
		}
	}
}
