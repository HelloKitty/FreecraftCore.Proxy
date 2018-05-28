using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GladNet;
using Nito.AsyncEx;

namespace FreecraftCore
{
	public sealed class InPlaceAsyncLockedNetworkMessageDispatchingStrategy<TPayloadWriteType, TPayloadReadType> : INetworkMessageDispatchingStrategy<TPayloadWriteType, TPayloadReadType>
		where TPayloadWriteType : class
		where TPayloadReadType : class
	{
		//TODO: Inject this instead? Make this a strategy decorator?
		private InPlaceNetworkMessageDispatchingStrategy<TPayloadWriteType, TPayloadReadType> DecoratedDisaDispatchingStrategy { get; }

		/// <summary>
		/// Async lock.
		/// </summary>
		private static AsyncLock LockObject { get; } = new AsyncLock();

		public InPlaceAsyncLockedNetworkMessageDispatchingStrategy()
		{
			DecoratedDisaDispatchingStrategy = new InPlaceNetworkMessageDispatchingStrategy<TPayloadWriteType, TPayloadReadType>();
		}

		public async Task DispatchNetworkMessage(SessionMessageContext<TPayloadWriteType, TPayloadReadType> context)
		{
			using(await LockObject.LockAsync())
				await DecoratedDisaDispatchingStrategy.DispatchNetworkMessage(context);
		}
	}
}
