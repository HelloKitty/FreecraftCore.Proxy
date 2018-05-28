using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class GameTestHandlerRegisterationModule : PayloadHandlerRegisterationModules<GamePacketPayload, GamePacketPayload>
	{
		/// <inheritdoc />
		public GameTestHandlerRegisterationModule() 
			: base(new List<PayloadHandlerRegisterationModule<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>(), new List<PayloadHandlerRegisterationModule<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>())
		{

		}

		public void AddServerHandlerModule([NotNull] PayloadHandlerRegisterationModule<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>> handlerModule)
		{
			if(handlerModule == null) throw new ArgumentNullException(nameof(handlerModule));

			var list = this.ServerMessageHandlerModules as List<PayloadHandlerRegisterationModule<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>;

			list.Add(handlerModule);
		}

		public void AddClientHanderModule([NotNull] PayloadHandlerRegisterationModule<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>> handlerModule)
		{
			if(handlerModule == null) throw new ArgumentNullException(nameof(handlerModule));

			var list = this.ClientMessageHandlerModules as List<PayloadHandlerRegisterationModule<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>>;

			list.Add(handlerModule);
		}
	}
}
