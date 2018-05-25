﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.Packet.Auth;
using GladNet;
using JetBrains.Annotations;
using Moq;

namespace FreecraftCore
{
	public abstract class ProxiedManagedClientSession<TPayloadWriteType, TPayloadReadType, TMessageContextType> : ManagedClientSession<TPayloadWriteType, TPayloadReadType> where TPayloadReadType : 
		class where TPayloadWriteType : class
		where TMessageContextType : IPeerMessageContext<TPayloadWriteType>
	{
		/// <summary>
		/// The message handling service for auth messages.
		/// </summary>
		private MessageHandlerService<TPayloadReadType, TPayloadWriteType, TMessageContextType> AuthMessageHandlerService { get; }

		public static IPeerRequestSendService<TPayloadWriteType> MockedInterceptor { get; }

		private IGenericMessageContextFactory<TPayloadWriteType, TMessageContextType> MessageContextFactory { get; }

		static ProxiedManagedClientSession()
		{
			//TODO: Implement when design issue in GladNet3 is fixed we can stop mocking this dependency.
			MockedInterceptor = Mock.Of<IPeerRequestSendService<TPayloadWriteType>>();
		}

		/// <inheritdoc />
		public ProxiedManagedClientSession(IManagedNetworkServerClient<TPayloadWriteType, TPayloadReadType> internalManagedNetworkClient, SessionDetails details,
			[NotNull] MessageHandlerService<TPayloadReadType, TPayloadWriteType, TMessageContextType> authMessageHandlerService, 
			IGenericMessageContextFactory<TPayloadWriteType, TMessageContextType> messageContextFactory)
			: base(internalManagedNetworkClient, details)
		{
			if(authMessageHandlerService == null) throw new ArgumentNullException(nameof(authMessageHandlerService));

			AuthMessageHandlerService = authMessageHandlerService;
			MessageContextFactory = messageContextFactory;
		}

		/// <inheritdoc />
		public override Task OnNetworkMessageRecieved(NetworkIncomingMessage<TPayloadReadType> message)
		{
			//TODO: How should we handle server not having interceptor
			return AuthMessageHandlerService.TryHandleMessage(MessageContextFactory.CreateMessageContext(Connection, SendService, Details), message);
		}

		/// <inheritdoc />
		protected override void OnSessionDisconnected()
		{
			//TODO: If the authserver disconnects us we should disconnect the proxied client too in the same way
		}
	}
}
