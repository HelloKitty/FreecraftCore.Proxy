using System;
using GladNet;
using JetBrains.Annotations;
using Moq;

namespace FreecraftCore
{
	public sealed class GenericMessageContextFactory<TPayloadWriteType, TPayloadReadType> : IGenericMessageContextFactory<TPayloadWriteType, IProxiedMessageContext<TPayloadWriteType, TPayloadReadType>> 
		where TPayloadWriteType : class where TPayloadReadType : class
	{
		private IManagedNetworkClient<TPayloadReadType, TPayloadWriteType> ProxyConnection { get; }

		private static IPeerRequestSendService<TPayloadWriteType> MockedPeerRequestService { get; } = Mock.Of<IPeerRequestSendService<TPayloadWriteType>>();

		public GenericMessageContextFactory([NotNull] IManagedNetworkClient<TPayloadReadType, TPayloadWriteType> proxyConnection)
		{
			ProxyConnection = proxyConnection ?? throw new ArgumentNullException(nameof(proxyConnection));
		}

		public IProxiedMessageContext<TPayloadWriteType, TPayloadReadType> CreateMessageContext(IConnectionService connectionService, IPeerPayloadSendService<TPayloadWriteType> sendService, SessionDetails details)
		{
			return new GenericProxiedMessageContext<TPayloadWriteType, TPayloadReadType>(ProxyConnection, connectionService, sendService, MockedPeerRequestService);
		}
	}
}