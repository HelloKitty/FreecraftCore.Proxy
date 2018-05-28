using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.Crypto;
using GladNet;

namespace FreecraftCore
{
	public interface ICombinedSessionPacketCryptoService
	{
		/// <summary>
		/// Indicates if the crypto service has been initialized.
		/// </summary>
		bool isInitialized { get; }

		SessionPacketCryptoService EncryptionService { get; }

		SessionPacketCryptoService DecryptionService { get; }
	}
}
