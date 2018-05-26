using System;
using System.Net;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Factory;
using GladNet;

namespace FreecraftCore
{
	class Program
	{
		static async Task Main(string[] args)
		{
			AuthTestHandlerRegisterationModule authHandlerModules = new AuthTestHandlerRegisterationModule();

			authHandlerModules.AddServerHandlerModule(new ManualAuthProxyTestSessionMessageHandlerRegisterationModule());
			authHandlerModules.AddClientHanderModule(new ManualAuthProxyTestClientMessageHandlerRegisterationModule());

			AuthenticationProxyApplicationBase appBase = new ManualAuthProxyTestApplicationBase(new NetworkAddressInfo(IPAddress.Parse("127.0.0.1"), 3724), new ConsoleLogger(LogLevel.All), authHandlerModules, new AuthTestNetworkSerializers());

			if(!appBase.StartServer())
			{
				Console.WriteLine("Failed to start proxy. Press any key to close.");
				Console.ReadKey();
				return;
			}

			Console.WriteLine("Starting proxy.");

			await appBase.BeginListening();
		}
	}
}
