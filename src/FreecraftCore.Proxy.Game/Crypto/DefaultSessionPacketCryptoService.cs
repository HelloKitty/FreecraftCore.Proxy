using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreecraftCore.Crypto;
using JetBrains.Annotations;

namespace FreecraftCore.Crypto
{
	public sealed class DefaultSessionPacketCryptoService : ICombinedSessionPacketCryptoService
	{
		private SRP6SessionKeyStore KeyStore { get; }

		public byte[] EncryptionHmacKey { get; }

		public byte[] DecryptionHmacKey { get; }

		/// <inheritdoc />
		public bool isInitialized => KeyStore.isInitialized;

		/// <inheritdoc />
		public DefaultSessionPacketCryptoService([NotNull] SRP6SessionKeyStore keyStore, byte[] encryptionHmacKey, byte[] decryptionHmacKey)
		{
			KeyStore = keyStore ?? throw new ArgumentNullException(nameof(keyStore));

			EncryptionHmacKey = encryptionHmacKey;
			DecryptionHmacKey = decryptionHmacKey;

			LazyEncryptionService = new Lazy<ICryptoServiceProvider>(() => new SessionARC4NPacketCryptoService(this.KeyStore.SessionKey.ToArray(), true, encryptionHmacKey), true);
			LazyDecryptionService = new Lazy<ICryptoServiceProvider>(() => new SessionARC4NPacketCryptoService(this.KeyStore.SessionKey.ToArray(), false, decryptionHmacKey), true);
		}

		private Lazy<ICryptoServiceProvider> LazyEncryptionService { get; }

		private Lazy<ICryptoServiceProvider> LazyDecryptionService { get; }

		/// <inheritdoc />
		public ICryptoServiceProvider EncryptionService => LazyEncryptionService.Value;

		/// <inheritdoc />
		public ICryptoServiceProvider DecryptionService => LazyDecryptionService.Value;
	}
}
