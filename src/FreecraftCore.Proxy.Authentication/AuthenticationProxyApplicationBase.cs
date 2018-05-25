using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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

			//The default handlers (Just forwards)
			builder.RegisterType<AuthDefaultClientRequestHandler>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterType<AuthDefaultSessionRequestHandler>()
				.AsImplementedInterfaces()
				.SingleInstance();

			builder.RegisterInstance(Logger)
				.As<ILog>();

			//The session handlers
			builder.RegisterType<MessageHandlerService<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>>()
				.As<MessageHandlerService<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>>()
				.SingleInstance();

			//The proxy client handlers
			builder.RegisterType<MessageHandlerService<AuthenticationServerPayload, AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext>>()
				.As<MessageHandlerService<AuthenticationServerPayload, AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext>>()
				.SingleInstance();

			//This registers all the authentication message handlers
			//They are not direct handlers but instead modules so that depending on the desired version different handlers can be used
			//this is especially useful in cases where you want multi-version mapping. Like 3.3.5->1.12.1
			ProduceServerMessageHandlerModules()
				.ToList()
				.ForEach(m => builder.RegisterModule(m));

			ProduceClientMessageHandlerModules()
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

		/// <summary>
		/// Implementers should return a collection of payload handler modules
		/// that they would like the authentication proxy, acting as the client, to use.
		/// Do NOT register server modules like this. Server modules with their own handlers are seperate.
		/// </summary>
		/// <returns></returns>
		protected abstract IReadOnlyCollection<PayloadHandlerRegisterationModule<AuthenticationServerPayload, AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext>> ProduceClientMessageHandlerModules();

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

		//This builds the pipeline for the outgoing connection. The connection the client's messages are to be proxied through.
		protected virtual IManagedNetworkServerClient<AuthenticationClientPayload, AuthenticationServerPayload> CreateOutgoingSessionPipeline(TcpClient client)
		{
			//TODO: We should handle endpoints better. Not static defined
			//We need to create an actual client to the server too.
			IManagedNetworkServerClient<AuthenticationClientPayload, AuthenticationServerPayload> serverProxyClient = new DotNetTcpClientNetworkClient(client)
				.AddHeaderlessNetworkMessageReading(Serializer)
				.For<AuthenticationServerPayload, AuthenticationClientPayload, IAuthenticationPayload>()
				.Build()
				.AsManagedSession();

			return serverProxyClient;
		}

		/// <inheritdoc />
		protected override ManagedClientSession<AuthenticationServerPayload, AuthenticationClientPayload> CreateIncomingSession(IManagedNetworkServerClient<AuthenticationServerPayload, AuthenticationClientPayload> client, SessionDetails details)
		{
			Logger.Info($"Recieved proxy connection from: {details.Address.AddressEndpoint.ToString()}:{details.Address.Port}");

			//TODO: Don't hardcode this
			//TcpClient proxyClientTcpClient = new TcpClient(Dns.GetHostEntry("logon.wowfeenix.com").HostName, 3724);
			TcpClient proxyClientTcpClient = new TcpClient("127.0.0.1", 5050);

			//We need to create the proxy client now too
			var proxyClient = CreateOutgoingSessionPipeline(proxyClientTcpClient);


			//TODO: Whenever a client session is created we should create a parallel client connection to the server we're in the middle of
			var connectionSession = new ProxiedAuthenticationConnectionSession(client, details, ServiceContainer.Resolve<MessageHandlerService<AuthenticationClientPayload, AuthenticationServerPayload, ProxiedAuthenticationSessionMessageContext>>(), new ProxiedSessionMessageContextFactory(proxyClient));

			//After the connection session is made with the message context factory that has a dependency on the proxyclient we must create the proxy client's session
			//which makes it easier to manage and it will have a dependency on the actual session

			var clientProxySession = new ProxiedAuthenticationClientSession(proxyClient, details, ServiceContainer.Resolve<MessageHandlerService<AuthenticationServerPayload, AuthenticationClientPayload, ProxiedAuthenticationClientMessageContext>>(), new ProxiedClientMessageContextFactory(client));

			//Now they can both communicate between eachother through the handler's message contexts
			//However since the AppBase only takes one session type, to maintain this session we need to manually start it
			//with the ManualClientConnectionLoop below. A copy-paste from the AppBase.
			Task.Factory.StartNew(async () => { await ManualStartClientConnectionLoop(proxyClientTcpClient, proxyClient, clientProxySession); })
				.ConfigureAwait(false);

			return connectionSession;
		}

		private async Task ManualStartClientConnectionLoop(TcpClient client, IManagedNetworkServerClient<AuthenticationClientPayload, AuthenticationServerPayload> internalNetworkClient, ManagedClientSession<AuthenticationClientPayload, AuthenticationServerPayload> networkSession)
		{
			//So that sessions invoking the disconnection can internally disconnect to
			networkSession.OnSessionDisconnection += (source, args) => internalNetworkClient.Disconnect();

			var dispatchingStrategy = new InPlaceNetworkMessageDispatchingStrategy<AuthenticationClientPayload, AuthenticationServerPayload>();

			try
			{
				while(client.Connected && internalNetworkClient.isConnected)
				{
					NetworkIncomingMessage<AuthenticationServerPayload> message = await internalNetworkClient.ReadMessageAsync(CancellationToken.None)
						.ConfigureAwait(false);

					//TODO: This will work for World of Warcraft since it requires no more than one packet
					//from the same client be handled at one time. However it limits throughput and maybe we should
					//handle this at a different level instead. 
					await dispatchingStrategy.DispatchNetworkMessage(new SessionMessageContext<AuthenticationClientPayload, AuthenticationServerPayload>(networkSession, message))
						.ConfigureAwait(false);
				}
			}
			catch(Exception e)
			{
				//TODO: Remove this console log
				Console.WriteLine($"[Error]: {e.Message}\n\nStack: {e.StackTrace}");
			}

			client.Dispose();

			//TODO: Should we tell the client something when it ends?
			networkSession.DisconnectClientSession();
		}
	}
}
