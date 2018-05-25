using System;
using System.Collections.Generic;
using System.Text;
using Binarysharp.MemoryManagement;
using Common.Logging;
using FreecraftCore.Packet.Auth;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class ManualAuthProxyTestApplicationBase : AuthenticationProxyApplicationBase
	{
		/// <inheritdoc />
		public ManualAuthProxyTestApplicationBase(NetworkAddressInfo serverAddress, [NotNull] ILog logger) 
			: base(serverAddress, logger)
		{
			
		}

		/// <inheritdoc />
		protected override IEnumerable<Type> ProduceAuthenticationPayloadTypes()
		{
			return new Type[] {typeof(AuthLogonChallengeRequest), typeof(AuthLogonChallengeResponse), typeof(AuthLogonProofResponse), typeof(AuthLogonProofRequest), typeof(AuthRealmListRequest), typeof(AuthRealmListResponse)};
		}

		/// <inheritdoc />
		protected override IReadOnlyCollection<PayloadHandlerRegisterationModule<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>> ProduceServerMessageHandlerModules()
		{
			var moduleList = new List<PayloadHandlerRegisterationModule<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>>(5);
			moduleList.Add(new ManualAuthProxyTestSessionMessageHandlerRegisterationModule());
			
			return moduleList;
		}

		/// <inheritdoc />
		protected override IReadOnlyCollection<PayloadHandlerRegisterationModule<AuthenticationServerPayload, AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext>> ProduceClientMessageHandlerModules()
		{
			var moduleList = new List<PayloadHandlerRegisterationModule<AuthenticationServerPayload, AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext>>(5);

			moduleList.Add(new ManualAuthProxyTestClientMessageHandlerRegisterationModule());

			return moduleList;
		}
	}
}
