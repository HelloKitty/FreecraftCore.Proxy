using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.Crypto;
using JetBrains.Annotations;

namespace FreecraftCore
{
	public sealed class OutgoingWoltkToVanillaCryptoService : ICombinedSessionPacketCryptoService
	{
		private SRP6SessionKeyStore KeyStore { get; }

		/// <inheritdoc />
		public bool isInitialized => KeyStore.isInitialized;

		/// <inheritdoc />
		public OutgoingWoltkToVanillaCryptoService([NotNull] SRP6SessionKeyStore keyStore)
		{
			KeyStore = keyStore ?? throw new ArgumentNullException(nameof(keyStore));

			LazyEncryptionService = new Lazy<ICryptoServiceProvider>(() => new SessionXORPacketCryptoService(true, KeyStore.SessionKey));
			LazyDecryptionService = new Lazy<ICryptoServiceProvider>(() => new SessionXORPacketCryptoService(false, KeyStore.SessionKey), true);
		}

		private Lazy<ICryptoServiceProvider> LazyEncryptionService { get; }

		private Lazy<ICryptoServiceProvider> LazyDecryptionService { get; }

		/// <inheritdoc />
		public ICryptoServiceProvider EncryptionService => LazyEncryptionService.Value;

		/// <inheritdoc />
		public ICryptoServiceProvider DecryptionService => LazyDecryptionService.Value;
	}
}
