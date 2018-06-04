using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using Fasterflect;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	[ServerPayloadHandler]
	public sealed class WotlkToVanillaGameAuthChallengeEventPayloadHandler : BaseGameServerPayloadHandler<SessionAuthChallengeEvent_Vanilla>
	{
		/// <inheritdoc />
		public WotlkToVanillaGameAuthChallengeEventPayloadHandler([NotNull] ILog logger)
			: base(logger)
		{

		}

		/// <inheritdoc />
		public override Task OnHandleMessage(IProxiedMessageContext<GamePacketPayload, GamePacketPayload> context, SessionAuthChallengeEvent_Vanilla payload)
		{
			//We need to transform this to the proper DTO.
			//The vanilla DTO is slightly different so we just construct a wotlk
			//version of it.
			//Console.WriteLine(payload.GetType().Name);
			//Console.WriteLine(payload.EventData.GetType().Name);

			return context.ProxyConnection.SendMessage(new SessionAuthChallengeEvent(payload.EventData));
		}
	}
}
