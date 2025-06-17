
using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi;

/// <summary>
/// Cipher algorithm for data encryption and decryption
/// </summary>
/// <remarks>
/// Equivalent to DOT11_CIPHER_ALGORITHM:
/// https://learn.microsoft.com/en-us/windows/win32/nativewifi/dot11-cipher-algorithm
/// </remarks>
public enum CipherAlgorithm
{
	/// <summary>
	/// No cipher algorithm is enabled or supported.
	/// </summary>
	None = 0,

	/// <summary>
	/// Wired Equivalent Privacy (WEP) algorithm with a cipher key of any length
	/// </summary>
	WEP,

	/// <summary>
	/// WEP algorithm with a 40-bit cipher key
	/// </summary>
	WEP_40,

	/// <summary>
	/// WEP algorithm with a 104-bit cipher key
	/// </summary>
	WEP_104,

	/// <summary>
	/// Temporal Key Integrity Protocol (TKIP) algorithm
	/// </summary>
	TKIP,

	/// <summary>
	/// AES-CCMP algorithm
	/// </summary>
	CCMP,

	/// <summary>
	/// BIP-CMAC-128 algorithm
	/// </summary>
	BIP,

	/// <summary>
	/// GCMP-128 algorithm
	/// </summary>	
	GCMP,

	/// <summary>
	/// GCMP-256 algorithm
	/// </summary>	
	GCMP_256,

	/// <summary>
	/// CCMP-256 algorithm
	/// </summary>	
	CCMP_256,

	/// <summary>
	/// BIP-GMAC-128 algorithm
	/// </summary>
	BIP_GMAC_128,

	/// <summary>
	/// BIP-GMAC-256 algorithm
	/// </summary>	
	BIP_GMAC_256,

	/// <summary>
	/// BIP-CMAC-256 algorithm
	/// </summary>	
	BIP_CMAC_256,

	/// <summary>
	/// Wi-Fi Protected Access (WPA) Use Group Key cipher suite
	/// </summary>
	WPA_USE_GROUP,

	/// <summary>
	/// Robust Security Network (RSN) Use Group Key cipher suite (not used)
	/// </summary>
	RSN_USE_GROUP,

	/// <summary>
	/// Indicates the start of the range that specifies proprietary cipher algorithms developed by an independent hardware vendor (IHV).
	/// </summary>
	IHV_START,

	/// <summary>
	/// Indicates the end of the range that specifies proprietary cipher algorithms developed by an independent hardware vendor (IHV).
	/// </summary>
	IHV_END
}

internal static class CipherAlgorithmConverter
{
	public static bool TryConvert(DOT11_CIPHER_ALGORITHM source, out CipherAlgorithm cipherAlgorithm)
	{
		switch (source)
		{
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_NONE:
				cipherAlgorithm = CipherAlgorithm.None;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WEP40:
				cipherAlgorithm = CipherAlgorithm.WEP_40;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_TKIP:
				cipherAlgorithm = CipherAlgorithm.TKIP;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_CCMP:
				cipherAlgorithm = CipherAlgorithm.CCMP;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WEP104:
				cipherAlgorithm = CipherAlgorithm.WEP_104;
				return true;

			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_BIP:
				cipherAlgorithm = CipherAlgorithm.BIP;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_GCMP:
				cipherAlgorithm = CipherAlgorithm.GCMP;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_GCMP_256:
				cipherAlgorithm = CipherAlgorithm.GCMP_256;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_CCMP_256:
				cipherAlgorithm = CipherAlgorithm.CCMP_256;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_BIP_GMAC_128:
				cipherAlgorithm = CipherAlgorithm.BIP_GMAC_128;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_BIP_GMAC_256:
				cipherAlgorithm = CipherAlgorithm.BIP_GMAC_256;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_BIP_CMAC_256:
				cipherAlgorithm = CipherAlgorithm.BIP_CMAC_256;
				return true;

			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WPA_USE_GROUP:
				cipherAlgorithm = CipherAlgorithm.WPA_USE_GROUP;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WEP:
				cipherAlgorithm = CipherAlgorithm.WEP;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_IHV_START:
				cipherAlgorithm = CipherAlgorithm.IHV_START;
				return true;
			case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_IHV_END:
				cipherAlgorithm = CipherAlgorithm.IHV_END;
				return true;
		}
		cipherAlgorithm = default;
		return false;
	}
}