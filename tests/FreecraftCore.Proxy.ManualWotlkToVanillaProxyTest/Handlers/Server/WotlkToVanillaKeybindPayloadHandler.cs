using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaKeybindPayloadHandler : GameServerPayloadConverterHandler<SMSG_ACTION_BUTTONS_Payload_Vanilla, SMSG_ACTION_BUTTONS_Payload>
	{
		/// <inheritdoc />
		public WotlkToVanillaKeybindPayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{

		}

		/// <inheritdoc />
		protected override SMSG_ACTION_BUTTONS_Payload ConvertToOutputPayload(SMSG_ACTION_BUTTONS_Payload_Vanilla payload)
		{
			//TODO: Add constant for Keybind length
			//TC always sends 1 so we'll send 1.
			return new SMSG_ACTION_BUTTONS_Payload(SMSG_ACTION_BUTTONS_Payload.State.Initial, payload.ButtonData.Concat(Enumerable.Repeat(new ActionButtonData(0), 144 - payload.ButtonData.Count)).ToArray());
		}
	}
}
