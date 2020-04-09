using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Fasterflect;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	[ServerPayloadHandler]
	public sealed class ServerAuthSessionEventPayloadCaptureHandler : BaseGameServerPayloadHandler<SMSG_AUTH_CHALLENGE_DTO_PROXY>
	{
		/// <inheritdoc />
		public ServerAuthSessionEventPayloadCaptureHandler([NotNull] ILog logger)
			: base(logger)
		{

		}

		/// <inheritdoc />
		public override async Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SMSG_AUTH_CHALLENGE_DTO_PROXY payload)
		{
			this.Logger.Info($"Recieved: {nameof(SMSG_AUTH_CHALLENGE_DTO_PROXY)}");
		}
	}
}
