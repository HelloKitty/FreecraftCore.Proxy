using System;
using System.Collections.Generic;
using System.Text;
using FreecraftCore.Packet;
using FreecraftCore.Packet.Auth;

namespace FreecraftCore
{
	/// <summary>
	/// Simplied Type alias for game registeration module for client handlers.
	/// </summary>
	public abstract class GameClientPayloadHandlerRegisterationModule : PayloadHandlerRegisterationModule<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>
	{
		protected GameClientPayloadHandlerRegisterationModule()
		{
			//Since game packet payloads are same for incoming and outgoing we need to
			//add the required attribute
			AddRequiredAttribute<ClientPayloadHandlerAttribute>();
		}
	}
}
