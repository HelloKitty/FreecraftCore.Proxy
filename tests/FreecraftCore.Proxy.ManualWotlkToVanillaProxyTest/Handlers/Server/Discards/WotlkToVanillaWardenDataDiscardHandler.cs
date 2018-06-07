using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class WotlkToVanillaWardenDataDiscardHandler : GameServerPayloadDiscardHandler<WardenDataEvent>
	{
		/// <inheritdoc />
		public WotlkToVanillaWardenDataDiscardHandler([NotNull] ILog logger) 
			: base(logger)
		{

		}
	}
}
