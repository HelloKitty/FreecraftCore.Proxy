using System;
using System.Collections.Generic;
using System.Text;

namespace FreecraftCore
{
	/// <summary>
	/// Attribute that hints that the handler is a client payload handler.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public sealed class ClientPayloadHandlerAttribute : Attribute
	{
		
	}
}
