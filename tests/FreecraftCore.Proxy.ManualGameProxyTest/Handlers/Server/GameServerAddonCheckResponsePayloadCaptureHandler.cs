using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class GameServerAddonCheckResponsePayloadCaptureHandler : GameServerPacketLoggerServerPayloadHandler<SMSG_ADDON_INFO_DTO_PROXY>
	{
		/// <inheritdoc />
		public GameServerAddonCheckResponsePayloadCaptureHandler([NotNull] ILog logger) 
			: base(logger, true)
		{
		}
	}
}
