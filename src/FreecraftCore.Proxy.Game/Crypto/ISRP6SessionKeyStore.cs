using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GladNet;
using JetBrains.Annotations;

namespace FreecraftCore
{
	/// <summary>
	/// The session key store.
	/// </summary>
	public class SRP6SessionKeyStore : ICryptoKeyInitializable<byte[]>
	{
		/// <summary>
		/// Indicates if the crypto store has been initialized.
		/// If it hasn't been initialized then <see cref="SessionKey"/>
		/// is not in a valid state.
		/// </summary>
		public bool isInitialized { get; private set; }

		/// <summary>
		/// The session key.
		/// </summary>
		public byte[] SessionKey { get; private set; }

		/// <inheritdoc />
		public void Initialize([NotNull] byte[] key)
		{
			//TODO: Verify key length
			if(key == null) throw new ArgumentNullException(nameof(key));
			SessionKey = key;
			isInitialized = true;
		}

		/// <inheritdoc />
		public void Uninitialize()
		{
			isInitialized = false;
		}
	}
}
