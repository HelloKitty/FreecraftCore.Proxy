using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Autofac;
using Common.Logging;
using FreecraftCore.Packet.Auth;
using FreecraftCore.Serializer;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public abstract class AuthenticationProxyApplicationBase : TcpServerServerApplicationBase<AuthenticationServerPayload, AuthenticationClientPayload>
	{
		/// <summary>
		/// Application logger.
		/// </summary>
		public ILog Logger { get; }

		private INetworkSerializationService Serializer { get; }

		private IContainer ServiceContainer { get; }

		/// <inheritdoc />
		protected AuthenticationProxyApplicationBase(NetworkAddressInfo serverAddress, [NotNull] ILog logger)
			: base(serverAddress)
		{
			if(serverAddress == null) throw new ArgumentNullException(nameof(serverAddress));
			if(logger == null) throw new ArgumentNullException(nameof(logger));

			Logger = logger;
			Serializer = new FreecraftCoreGladNetSerializerAdapter(CreateSerializer());
			ServiceContainer = BuildServiceContainer();
		}

		private IContainer BuildServiceContainer()
		{
			ContainerBuilder builder = new ContainerBuilder();

			builder.RegisterInstance(Serializer)
				.As<INetworkSerializationService>();

			builder.RegisterType<AuthDefaultRequestHandler>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterInstance(Logger)
				.As<ILog>();

			builder.RegisterType<MessageHandlerService<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>>()
				.As<MessageHandlerService<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>>()
				.SingleInstance();

			//This registers all the authentication message handlers
			//They are not direct handlers but instead modules so that depending on the desired version different handlers can be used
			//this is especially useful in cases where you want multi-version mapping. Like 3.3.5->1.12.1
			ProduceServerMessageHandlerModules()
				.ToList()
				.ForEach(m => builder.RegisterModule(m));
			

			return builder.Build();
		}

		//TODO: We should move this externally so that registeration of packet/payload types can support different versions
		private ISerializerService CreateSerializer()
		{
			SerializerService serializer = new SerializerService();

			ProduceAuthenticationPayloadTypes()
				.ToList()
				.ForEach(t => serializer.RegisterType(t));

			serializer.Compile();

			return serializer;
		}

		/// <summary>
		/// Implementers should return a collection of all payload types that the proxy should
		/// be know how to serialize or deserialize.
		/// </summary>
		/// <returns></returns>
		protected abstract IEnumerable<Type> ProduceAuthenticationPayloadTypes();

		/// <summary>
		/// Implementers should return a collection of payload handler modules
		/// that they would like the authentication proxy, acting as the server, to use.
		/// Do NOT register client modules like this. Client modules with their own handlers are seperate.
		/// </summary>
		/// <returns></returns>
		protected abstract IReadOnlyCollection<PayloadHandlerRegisterationModule<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>> ProduceServerMessageHandlerModules();

		/// <inheritdoc />
		protected override bool IsClientAcceptable(TcpClient tcpClient)
		{
			return true;
		}

		/// <inheritdoc />
		protected override IManagedNetworkServerClient<AuthenticationServerPayload, AuthenticationClientPayload> CreateIncomingSessionPipeline(TcpClient client)
		{

			//TODO: Are any details actually valuable here?
			if(Logger.IsInfoEnabled)
				Logger.Info($"Client connected to proxy.");

			//The auth server is encryptionless and 'headerless' so we do not need to support that on the proxy for the auth server
			IManagedNetworkServerClient<AuthenticationServerPayload, AuthenticationClientPayload> managedClient = new DotNetTcpClientNetworkClient(client)
				.AddHeaderlessNetworkMessageReading(Serializer)
				.For<AuthenticationClientPayload, AuthenticationServerPayload, IAuthenticationPayload>()
				.Build()
				.AsManagedSession(Logger);

			return managedClient;
		}

		/// <inheritdoc />
		protected override ManagedClientSession<AuthenticationServerPayload, AuthenticationClientPayload> CreateIncomingSession(IManagedNetworkServerClient<AuthenticationServerPayload, AuthenticationClientPayload> client, SessionDetails details)
		{
			Logger.Info($"Recieved proxy connection from: {details.Address.AddressEndpoint.ToString()}:{details.Address.Port}");

			//TODO: We should handle endpoints better. Not static defined
			//We need to create an actual client to the server too.
			IManagedNetworkClient<AuthenticationClientPayload, AuthenticationServerPayload> serverProxyClient = new DotNetTcpClientNetworkClient(new TcpClient())
				.AddHeaderlessNetworkMessageReading(Serializer)
				.For<AuthenticationServerPayload, AuthenticationClientPayload, IAuthenticationPayload>()
				.Build()
				.AsManaged();

			serverProxyClient.Connect("127.0.0.1", 5050);

			//TODO: Whenever a client session is created we should create a parallel client connection to the server we're in the middle of
			return new ProxiedAuthenticationConnectionSession(client, details, ServiceContainer.Resolve<MessageHandlerService<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>>(), serverProxyClient);
		}
	}
}
