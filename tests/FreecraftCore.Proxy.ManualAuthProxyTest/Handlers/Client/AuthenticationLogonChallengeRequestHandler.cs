using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	[ClientPayloadHandler]
	public sealed class AuthenticationLogonChallengeRequestHandler : BaseAuthenticationClientPayloadHandler<AuthLogonChallengeRequest>
	{
		/// <inheritdoc />
		public override Task HandleMessage(IProxiedMessageContext<AuthenticationServerPayload, AuthenticationClientPayload> context, AuthLogonChallengeRequest payload)
		{
			return context.ProxyConnection.SendMessage(new AuthLogonChallengeRequest(payload.Protocol, GameType.WoW, ExpansionType.WrathOfTheLichKing, 3, 5, ClientBuild.Wotlk_3_3_5a, PlatformType.x64, OperatingSystemType.Mac, LocaleType.enUS, payload.IP, payload.Identity));
		}
	}
}
