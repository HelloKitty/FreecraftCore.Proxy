using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using GladNet;
using JetBrains.Annotations;
using Fasterflect;

namespace FreecraftCore
{
	public sealed class AuthenticationRealmListResponseServerHandler : BaseAuthenticationServerPayloadHandler<AuthRealmListResponse>
	{
		/// <inheritdoc />
		public AuthenticationRealmListResponseServerHandler([NotNull] ILog logger) 
			: base(logger)
		{

		}

#pragma warning disable AsyncFixer01 // Unnecessary async/await usage
		/// <inheritdoc />
		public override async Task HandleMessage(IProxiedMessageContext<AuthenticationClientPayload, AuthenticationServerPayload> context, AuthRealmListResponse payload)
		{
			if(Logger.IsInfoEnabled)
				Logger.Info("Entered captures realm list packet handler.");

			foreach(var realm in payload.Realms)
				if(Logger.IsInfoEnabled)
					Logger.Info($"Realm Listing: {realm} Address: {realm.Information.RealmAddress.RealmIP}:{realm.Information.RealmAddress.Port}");

			//Rewrite the response to point to our proxy
			string realmString = payload.Realms.First().Information.RealmAddress.GetPropertyValue("RealmEndpointInformation") as string;
			string newRealmString = "127.0.0.1:8085";

			//Also need to set t he new size
			payload.PayloadSize = (ushort)(payload.PayloadSize - (realmString.Length - newRealmString.Length));

			if(Logger.IsInfoEnabled)
				Logger.Info($"AddressString: {realmString}");

			//We should also modify the realm info
			RealmInfo realmInfo = RebuildRealmInfo(payload.Realms.First(), newRealmString);

			await context.ProxyConnection.SendMessage(new AuthRealmListResponse(payload.PayloadSize, new RealmInfo[1] { realmInfo }));
		}
#pragma warning restore AsyncFixer01 // Unnecessary async/await usage

		private static RealmInfo RebuildRealmInfo(RealmInfo info, string newAddress)
		{
			DefaultRealmInformation information = info.Information as DefaultRealmInformation;

			DefaultRealmInformation newRealmDefaultInfo = new DefaultRealmInformation(information.Flags & ~RealmFlags.Offline, information.RealmString, new RealmEndpoint(newAddress), information.PopulationLevel, info.Information.CharacterCount, information.RealmTimeZone, information.RealmId);

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
