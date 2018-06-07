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

			GameProxyApplicationBase appBase = new GameProxyApplicationBase(new NetworkAddressInfo(IPAddress.Parse("127.0.0.1"), 8085),
				new NetworkAddressInfo(Dns.GetHostAddresses("ec2-18-218-255-202.us-east-2.compute.amazonaws.com").First(), 8085),
				new ConsoleLogger(LogLevel.All), gameHandlerModules, 
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
