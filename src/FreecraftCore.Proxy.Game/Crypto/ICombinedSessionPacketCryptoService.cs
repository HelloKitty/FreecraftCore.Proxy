using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.Crypto;
using GladNet;
using ICryptoServiceProvider = FreecraftCore.Crypto.ICryptoServiceProvider;

namespace FreecraftCore
{
	public interface ICombinedSessionPacketCryptoService
	{
		/// <summary>
		/// Indicates if the crypto service has been initialized.
		/// </summary>
		bool isInitialized { get; }

		ICryptoServiceProvider EncryptionService { get; }

		ICryptoServiceProvider DecryptionService { get; }
	}
}
