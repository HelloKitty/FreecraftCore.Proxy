using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac.Builder;
using Reflect.Extent;

namespace FreecraftCore
{
	/// <summary>
	/// Simplied Type alias for game registeration module for server handlers.
	/// </summary>
	public abstract class GameServerPayloadHandlerRegisterationModule : PayloadHandlerRegisterationModule<GamePacketPayload, GamePacketPayload, IProxiedMessageContext<GamePacketPayload, GamePacketPayload>>
	{
		/// <inheritdoc />
		protected override IEnumerable<Type> OnProcessHandlerTypes(IEnumerable<Type> handlerTypes)
		{
			//Since game packet payloads are same for incoming and outgoing we need to
			//add the required attribute
			return base.OnProcessHandlerTypes(handlerTypes)
				.Where(t => t.GetCustomAttribute<ServerPayloadHandlerAttribute>(true) != null);
		}
	}
}
