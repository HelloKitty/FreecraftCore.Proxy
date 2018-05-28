using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common.Logging;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public class GameProxyApplicationBase : ProxiedTcpServerApplicationBase<GamePacketPayload, GamePacketPayload>
	{
		/// <inheritdoc />
		public GameProxyApplicationBase(NetworkAddressInfo listenerAddress, NetworkAddressInfo proxyToEndpointAddress, [NotNull] ILog logger, PayloadHandlerRegisterationModules<GamePacketPayload, GamePacketPayload> handlerModulePair, NetworkSerializerServicePair serializers) 
			: base(listenerAddress, proxyToEndpointAddress, logger, handlerModulePair, serializers)
		{

		}

		//TODO: Maybe not hard code this for different server or client versions
		/// <summary>
		/// Reverse engineered from the client.
		/// (Found in Jackpoz's bot)
		/// </summary>
		private static readonly byte[] encryptionKey = new byte[]
		{
			0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5,
			0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE
		};

		/// <summary>
		/// Reverse engineered from the client.
		/// (Found in Jackpoz's bot)
		/// </summary>
		private static readonly byte[] decryptionKey = new byte[]
		{
			0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA,
			0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57
		};

		/// <inheritdoc />
		protected override IManagedNetworkServerClient<GamePacketPayload, GamePacketPayload> BuildIncomingSessionManagedClient(NetworkClientBase clientBase, INetworkSerializationService serializeService)
		{
			//TODO: Are any details actually valuable here?
			if(Logger.IsInfoEnabled)
				Logger.Info($"Client connected to proxy.");

			DefaultSessionPacketCryptoService cryptoService = new DefaultSessionPacketCryptoService(ServiceContainer.Resolve<SRP6SessionKeyStore>(), decryptionKey.ToArray(), encryptionKey.ToArray());

			var wowClientReadServerWrite = new WoWClientReadServerWriteProxyPacketPayloadReaderWriterDecorator<NetworkClientBase, GamePacketPayload, GamePacketPayload, IGamePacketPayload>(clientBase, serializeService, cryptoService);

			return new ManagedNetworkServerClient<WoWClientReadServerWriteProxyPacketPayloadReaderWriterDecorator<NetworkClientBase, GamePacketPayload, GamePacketPayload, IGamePacketPayload>, GamePacketPayload, GamePacketPayload>(wowClientReadServerWrite, Logger);
		}

		/// <inheritdoc />
		protected override IManagedNetworkClient<GamePacketPayload, GamePacketPayload> BuildOutgoingSessionManagedClient(NetworkClientBase clientBase, INetworkSerializationService serializeService)
		{
			DefaultSessionPacketCryptoService cryptoService = new DefaultSessionPacketCryptoService(ServiceContainer.Resolve<SRP6SessionKeyStore>(), encryptionKey.ToArray(), decryptionKey.ToArray());

			var wowClientReadServerWrite = new WoWClientWriteServerReadProxyPacketPayloadReaderWriterDecorator<NetworkClientBase, GamePacketPayload, GamePacketPayload, IGamePacketPayload>(clientBase, serializeService, cryptoService);

			return new ManagedNetworkServerClient<WoWClientWriteServerReadProxyPacketPayloadReaderWriterDecorator<NetworkClientBase, GamePacketPayload, GamePacketPayload, IGamePacketPayload>, GamePacketPayload, GamePacketPayload>(wowClientReadServerWrite, Logger);
		}

		/// <inheritdoc />
		protected override GenericProxiedManagedClientSession<TWriteType, TReadType> GenerateClientFromLifetimeScope<TWriteType, TReadType>(ILifetimeScope lifetimeScope)
		{
			//TODO: This is a hack, we need a better way. The only way we know this is the case is because of details elsewhere
			IManagedNetworkServerClient<TWriteType, TReadType> client = lifetimeScope.Resolve<IManagedNetworkServerClient<TWriteType, TReadType>>();

			bool isServer = (client is ManagedNetworkServerClient<WoWClientWriteServerReadProxyPacketPayloadReaderWriterDecorator<NetworkClientBase, GamePacketPayload, GamePacketPayload, IGamePacketPayload>, GamePacketPayload, GamePacketPayload>);

			//If it's the server one
			return new GenericProxiedManagedClientSession<TWriteType, TReadType>(
					client, lifetimeScope.Resolve<SessionDetails>(),
					lifetimeScope.ResolveNamed<MessageHandlerService<TReadType, TWriteType, IProxiedMessageContext<TWriteType, TReadType>>>(isServer ? "Server" : "Client"),
					lifetimeScope.Resolve<IGenericMessageContextFactory<TWriteType, IProxiedMessageContext<TWriteType, TReadType>>>());
		}

		//TODO: Refactor
		/// <inheritdoc />
		protected override void RegisterMessageHandlerServices(ContainerBuilder builder)
		{
			//TODO: FIX HANDLERS. CAN"T REGISTER INDEPENDENT SERVER/CLIENT
			//We have to do all this manually because the packet payload types do not differ between client and server.
			builder.Register<MessageHandlerService<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>(context =>
				{
					IPeerMessageHandler<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>[] handlers
						= context.Resolve<IEnumerable<IPeerMessageHandler<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>>()
							//.Where(h => h.GetType().GetCustomAttribute<ServerPayloadHandlerAttribute>(true) != null)
							.ToArray();

				return new MessageHandlerService<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>(handlers, context.Resolve<GameDefaultServerResponseHandler>());
			})
				.Named<MessageHandlerService<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>("Server")
				.SingleInstance();

			//The proxy client handlers
			builder.Register<MessageHandlerService<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>(context =>
				{
					IPeerMessageHandler<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>[] handlers
						= context.Resolve<IEnumerable<IPeerMessageHandler<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>>()
							//.Where(h => h.GetType().GetCustomAttribute<ClientPayloadHandlerAttribute>(true) != null)
							.ToArray();

					Console.WriteLine($"Found: {handlers.Length} many client handlers.");

					return new MessageHandlerService<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>(handlers, context.Resolve<GameDefaultClientRequestHandler>());
				})
				.Named<MessageHandlerService<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>("Client")
				.SingleInstance();
		}

		/// <inheritdoc />
		protected override ContainerBuilder RegisterHandlerDependencies(ContainerBuilder builder)
		{
			//The default handlers (Just forwards)
			builder.RegisterType<GameDefaultServerResponseHandler>()
				.AsImplementedInterfaces()
				.AsSelf()
				.SingleInstance();

			builder.RegisterType<GameDefaultClientRequestHandler>()
				.AsImplementedInterfaces()
				.AsSelf()
				.SingleInstance();

			builder.RegisterType<SRP6SessionKeyStore>()
				.AsSelf()
				.AsImplementedInterfaces()
				.SingleInstance();

			return builder;
		}
	}
}
