using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using GladNet;

namespace FreecraftCore.Proxy.ManualWotlkToVanillaProxyTest
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Task.Factory.StartNew(async () => await FreecraftCore.Auth.Program.Main(null).ConfigureAwait(false), TaskCreationOptions.LongRunning);


			//Start the wotlk to vanilla proxy
			GameTestHandlerRegisterationModule gameHandlerModules = new GameTestHandlerRegisterationModule();

			gameHandlerModules.AddServerHandlerModule(new ManualGameProxyTestSessionMessageHandlerRegisterationModule());
			gameHandlerModules.AddClientHanderModule(new ManualGameProxyTestClientMessageHandlerRegisterationModule());

			//We need the wotlktovanilla overriden version
			GameProxyApplicationBase appBase = new WotlkToVanillaProxyAppBase(new NetworkAddressInfo(IPAddress.Parse("127.0.0.1"), 8085),
				new NetworkAddressInfo(IPAddress.Parse("185.140.120.35"), 58085),
				new ConsoleLogger(LogLevel.All), gameHandlerModules,
				new WotlkToVanillaGameTestNetworkSerializers());

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
