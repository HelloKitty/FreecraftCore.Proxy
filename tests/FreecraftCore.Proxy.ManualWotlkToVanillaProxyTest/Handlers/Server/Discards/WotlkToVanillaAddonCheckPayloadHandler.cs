using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// We just want to discard <see cref="SMSG_ADDON_INFO_Payload_Vanilla"/>
	/// because it cannot work for wotlk. We don't capture it and forward the
	/// wotlk version either because it's optional that this packet is sent on 1.12.1 and not
	/// 3.3.5 I think. So just discard
	/// </summary>
	public sealed class WotlkToVanillaAddonCheckPayloadHandler : GameServerPayloadDiscardHandler<SMSG_ADDON_INFO_Payload_Vanilla>
	{
		/// <inheritdoc />
		public WotlkToVanillaAddonCheckPayloadHandler([NotNull] ILog logger) 
			: base(logger)
		{
		}
	}
}
