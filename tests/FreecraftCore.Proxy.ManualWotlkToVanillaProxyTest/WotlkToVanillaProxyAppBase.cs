using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
			//TODO: Support .NET Core global exception handling
			AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
			{
				Exception e = args.ExceptionObject as Exception;
				Logger.Fatal($"Unhandled Exception: {e.GetType().Name} Message: {e.Message} \n\n Stack: {e.StackTrace}");
			};
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
		protected override ContainerBuilder RegisterHandlerDependencies(ContainerBuilder builder)
		{
			base.RegisterHandlerDependencies(builder);

			//Register all the type converters in the assembly
			foreach(Type t in GetAllTypesImplementingOpenGenericType(typeof(ITypeConverterProvider<,>), typeof(WotlkToVanillaMovementInfoTypeConverter).Assembly))
				builder.RegisterType(t)
					.AsSelf()
					.AsImplementedInterfaces()
					.SingleInstance();

			return builder;
		}

		public static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType, Assembly assembly)
		{
			return from x in assembly.GetTypes()
				from z in x.GetInterfaces()
				let y = x.BaseType
				where
					(y != null && y.IsGenericType &&
						openGenericType.IsAssignableFrom(y.GetGenericTypeDefinition())) ||
					(z.IsGenericType &&
						openGenericType.IsAssignableFrom(z.GetGenericTypeDefinition()))
				select x;
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

			//These two handlers are special, they handle movement.
			builder.RegisterType<WotlkToVanillaClientMovementPayloadHandler>()
				.Named<IPeerMessageHandler<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>("Client")
				.SingleInstance();

			builder.RegisterType<WotlkToVanillaServerMovementPayloadHandler>()
				.Named<IPeerMessageHandler<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>("Server")
				.SingleInstance();

			return builder;
		}
	}
}
