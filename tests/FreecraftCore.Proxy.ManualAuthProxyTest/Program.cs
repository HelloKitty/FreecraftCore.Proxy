using System;
using System.Net;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Common.Logging;
using Common.Logging.Factory;
using GladNet;

namespace FreecraftCore.Auth
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			AuthTestHandlerRegisterationModule authHandlerModules = new AuthTestHandlerRegisterationModule();

			authHandlerModules.AddServerHandlerModule(new ManualAuthProxyTestSessionMessageHandlerRegisterationModule());
			authHandlerModules.AddClientHanderModule(new ManualAuthProxyTestClientMessageHandlerRegisterationModule());

			AuthenticationProxyApplicationBase appBase = new AuthenticationProxyApplicationBase(new NetworkAddressInfo(IPAddress.Parse("127.0.0.1"), 3724), 
				new NetworkAddressInfo(IPAddress.Parse("127.0.0.1"), 5050), new FreecraftCore.ConsoleLogger(LogLevel.All), authHandlerModules, 
				new AuthTestNetworkSerializers());

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
