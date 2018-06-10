using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaDestroyObjectPayloadHandler : GameServerPayloadConverterHandler<SMSG_DESTROY_OBJECT_Payload_Vanilla, SMSG_DESTROY_OBJECT_Payload>
	{
		/// <inheritdoc />
		public WotlkToVanillaDestroyObjectPayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{
		}

		/// <inheritdoc />
		protected override SMSG_DESTROY_OBJECT_Payload ConvertToOutputPayload(SMSG_DESTROY_OBJECT_Payload_Vanilla payload)
		{
			//TODO: How can we decide if it is for death? Could we track some packet data?
			return new SMSG_DESTROY_OBJECT_Payload(payload.DestroyedObject, false);
		}
	}
}
