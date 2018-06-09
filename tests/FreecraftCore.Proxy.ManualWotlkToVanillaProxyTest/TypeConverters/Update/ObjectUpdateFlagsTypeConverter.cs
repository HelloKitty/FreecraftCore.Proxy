using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreecraftCore
{
	//TODO: Support wotlk to vanilla
	public sealed class ObjectUpdateFlagsTypeConverter : ITypeConverterProvider<ObjectUpdateFlags_Vanilla, ObjectUpdateFlags>
	{
		/// <inheritdoc />
		public ObjectUpdateFlags Convert(ObjectUpdateFlags_Vanilla fromFlags)
		{
			ObjectUpdateFlags flags = 0;

			//This means we need to include the 32bit transport time.
			if(fromFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_TRANSPORT))
				flags |= ObjectUpdateFlags.UPDATEFLAG_TRANSPORT;

			if(fromFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_LIVING))
				flags |= ObjectUpdateFlags.UPDATEFLAG_LIVING;

			//I think this means we have to set the transport object if living too.
			if(fromFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_HAS_POSITION))
				flags |= ObjectUpdateFlags.UPDATEFLAG_POSITION;

			//TODO: Remove stationary hack
			if(fromFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_SELF))
				flags |= ObjectUpdateFlags.UPDATEFLAG_SELF;

			//This is odd, but they will send the guid of the target if this flag is enabled
			//So I assume we have to do the same here.
			if(fromFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_FULLGUID))
				flags |= ObjectUpdateFlags.UPDATEFLAG_HAS_TARGET;

			//Mangos sends the highGuid (32bit int) of the current unit this is the update for.
			//But TC just sends 0? So I gurss we should do that. It's unknown either way.
			if(fromFlags.HasFlag(ObjectUpdateFlags_Vanilla.UPDATEFLAG_HIGHGUID))
				flags |= ObjectUpdateFlags.UPDATEFLAG_UNKNOWN;

			return flags;
		}
	}
}
