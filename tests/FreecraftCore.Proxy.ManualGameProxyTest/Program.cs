using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Factory;
using GladNet;

namespace FreecraftCore.Game
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			GameTestHandlerRegisterationModule gameHandlerModules = new GameTestHandlerRegisterationModule();

			gameHandlerModules.AddServerHandlerModule(new ManualGameProxyTestSessionMessageHandlerRegisterationModule());
			gameHandlerModules.AddClientHanderModule(new ManualGameProxyTestClientMessageHandlerRegisterationModule());

			GameProxyApplicationBase appBase = new WotlkGameProxyApplicationBase(new NetworkAddressInfo(IPAddress.Parse("127.0.0.1"), 8085),
				new NetworkAddressInfo(IPAddress.Parse("51.68.68.236"), 8906),
				new AggergateCommonLoggingLogger(new ConsoleLogger(LogLevel.All), new FileLogger()), gameHandlerModules, 
				new GameTestNetworkSerializers());

			if(!appBase.StartServer())
			{
				Console.WriteLine("Failed to start proxy. Press any key to close.");
				Console.ReadKey();
				return;
			}

			Console.WriteLine("Starting game proxy.");

			await appBase.BeginListening();
		}
	}
}
