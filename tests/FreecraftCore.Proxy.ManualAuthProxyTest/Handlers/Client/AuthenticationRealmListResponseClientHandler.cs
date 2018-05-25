using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.API.Common;
using FreecraftCore.Packet.Auth;
using FreecraftCore.Packet.Common;
using GladNet;

namespace FreecraftCore
{
	public sealed class AuthenticationRealmListResponseClientHandler : IPeerPayloadSpecificMessageHandler<AuthRealmListResponse, AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext>
	{
#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
									/// <inheritdoc />
		public async Task HandleMessage(ProxiedAuthenticationClientMessageContext context, AuthRealmListResponse payload)
		{
			await context.ProxyClient.SendMessage(payload);
			return;
			//We have to rebuild the realm list to point to the proxy AND to adjust client build number

			//TODO: Do real data transformation instead of test transformation
			AuthRealmListResponse newRealmListResponse = new AuthRealmListResponse(payload.PayloadSize, new RealmInfo[] { RebuildRealmInfo(payload.Realms.First()) });

			await context.ProxyClient.SendMessage(newRealmListResponse);
		}
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage

		private static RealmInfo RebuildRealmInfo(RealmInfo info)
		{
			DefaultRealmInformation information = info.Information as DefaultRealmInformation;

			DefaultRealmInformation newRealmDefaultInfo = new DefaultRealmInformation(information.Flags & ~RealmFlags.Offline, new string(information.RealmString.Reverse().ToArray()), information.RealmAddress, information.PopulationLevel, 25, information.RealmTimeZone, information.RealmId);

			if(!info.HasBuildInformation)
				return new RealmInfo(info.RealmType, info.isLocked, newRealmDefaultInfo);
			else
			{
				//new RealmBuildInformation(ExpansionType.Vanilla, 12, 1, 5875)
				RealmBuildInformation buildInfo = info.BuildInfo;
				
				return new RealmInfo(info.RealmType, false, newRealmDefaultInfo, new RealmBuildInformation(ExpansionType.WrathOfTheLichKing, 3, 5, 12340));
			}
		}
	}
}
