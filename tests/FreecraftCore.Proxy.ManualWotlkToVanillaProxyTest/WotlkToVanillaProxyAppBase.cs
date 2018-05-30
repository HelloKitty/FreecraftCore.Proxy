using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Common.Logging;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaProxyAppBase : GameProxyApplicationBase
	{
		/// <inheritdoc />
		public WotlkToVanillaProxyAppBase(NetworkAddressInfo listenerAddress, NetworkAddressInfo proxyToEndpointAddress, [NotNull] ILog logger, PayloadHandlerRegisterationModules<GamePacketPayload, GamePacketPayload> handlerModulePair, NetworkSerializerServicePair serializers) : base(listenerAddress, proxyToEndpointAddress, logger, handlerModulePair, serializers)
		{

		}

		//TODO: Redo this design so we can inject this somehow.
		/// <inheritdoc />
		protected override ICombinedSessionPacketCryptoService BuildOutgoingPacketCryptoService(SRP6SessionKeyStore keyStore)
		{
			return new OutgoingWoltkToVanillaCryptoService(keyStore);
		}

		//TODO: Redesign this so we can replace just the reader/writer instead of the whole method copy/pasted
		/// <inheritdoc />
		protected override IManagedNetworkClient<GamePacketPayload, GamePacketPayload> BuildOutgoingSessionManagedClient(NetworkClientBase clientBase, INetworkSerializationService serializeService)
		{
			SRP6SessionKeyStore keyStore = ServiceContainer.Resolve<SRP6SessionKeyStore>();
			ICombinedSessionPacketCryptoService cryptoService = BuildOutgoingPacketCryptoService(keyStore);

			var wowClientReadServerWrite = new VanillaWoWClientReadServerWriteProxyPacketPayloadReaderWriterDecorator<NetworkClientBase>(clientBase, serializeService, cryptoService);

			return new ManagedNetworkServerClient<WoWClientWriteServerReadProxyPacketPayloadReaderWriterDecorator<NetworkClientBase, GamePacketPayload, GamePacketPayload, IGamePacketPayload>, GamePacketPayload, GamePacketPayload>(wowClientReadServerWrite, Logger);
		}

		/// <inheritdoc />
		protected override ContainerBuilder RegisterDefaultHandlers(ContainerBuilder builder)
		{
			builder.RegisterType<WotlkToVanillaGameDefaultServerRequestPayloadHandler>()
				.As<GameDefaultServerResponseHandler>()
				.AsImplementedInterfaces()
				.AsSelf()
				.SingleInstance();

			builder.RegisterType<WotlkToVanillaGameDefaultClientRequestPayloadHandler>()
				.As<GameDefaultClientRequestHandler>()
				.AsImplementedInterfaces()
				.AsSelf()
				.SingleInstance();

			return builder;
		}
	}
}
