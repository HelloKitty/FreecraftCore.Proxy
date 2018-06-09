using System;
using System.Collections.Generic;
using System.Globalization;
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
	public class WotlkGameProxyApplicationBase : GameProxyApplicationBase
	{
		/// <inheritdoc />
		public WotlkGameProxyApplicationBase(NetworkAddressInfo listenerAddress, NetworkAddressInfo proxyToEndpointAddress, [NotNull] ILog logger, PayloadHandlerRegisterationModules<GamePacketPayload, GamePacketPayload> handlerModulePair, NetworkSerializerServicePair serializers) 
			: base(listenerAddress, proxyToEndpointAddress, logger, handlerModulePair, serializers)
		{

		}

		/// <inheritdoc />
		protected override ContainerBuilder RegisterDefaultHandlers(ContainerBuilder builder)
		{
			base.RegisterDefaultHandlers(builder);

			RegisterCaptureHandlers(builder);

			return builder;
		}

		private static void RegisterCaptureHandlers(ContainerBuilder builder)
		{
			//Register a capture handler for every unimplemented proxy DTO
			foreach(Type t in GamePacketStubMetadataMarker.GamePacketPayloadStubTypes)
			{
				if(t != typeof(CMSG_AUTH_SESSION_DTO_PROXY))
					builder.RegisterType(typeof(GamePacketLoggerPayloadHandler<>).MakeGenericType(t))
						.AsSelf()
						.AsImplementedInterfaces()
						.SingleInstance();
			}

			foreach(Type t in GamePacketStubMetadataMarker.GamePacketPayloadStubTypes)
			{
				if(t != typeof(CMSG_AUTH_SESSION_DTO_PROXY))
				{
					RegisterNewHandlerOfTypeWithName(builder, t, "Server");
					RegisterNewHandlerOfTypeWithName(builder, t, "Client");
				}
			}
		}

		private static void RegisterNewHandlerOfTypeWithName(ContainerBuilder builder, Type t, string name)
		{
			builder.Register(c =>
			{
				object obj = c.Resolve(typeof(GamePacketLoggerPayloadHandler<>).MakeGenericType(t));

				Type handlerType = typeof(TrySemanticsBasedOnTypePeerMessageHandler<,,,>).MakeGenericType(typeof(GamePacketPayload), typeof(GamePacketPayload), t, typeof(IProxiedMessageContext<GamePacketPayload, GamePacketPayload>));

				return Activator.CreateInstance(handlerType, (dynamic)obj);
			})
				.Named<IPeerMessageHandler<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>(name)
				.SingleInstance();
		}
	}
}
