using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Enumerated type defines a cipher algorithm for data encryption and decryption
	/// </summary>
	public enum CipherAlgorithm
	{
		/// <summary>
		/// Specifies that no cipher algorithm is enabled or supported
		/// </summary>
		None,

		/// <summary>
		/// Specifies a Wired Equivalent Privacy (WEP) algorithm, which is the RC4-based algorithm that is specified in the 802.11-1999 standard
		/// </summary>
		WEP40,

		/// <summary>
		/// Specifies a Temporal Key Integrity Protocol (TKIP) algorithm, which is the RC4-based cipher suite that is based on the algorithms that are defined in the WPA specification and IEEE 802.11i-2004 standard
		/// </summary>
		TKIP,

		/// <summary>
		/// Specifies an AES-CCMP algorithm, as specified in the IEEE 802.11i-2004 standard and RFC 3610
		/// </summary>
		CCMP,

		/// <summary>
		/// Specifies a WEP cipher algorithm with a 104-bit cipher key
		/// </summary>
		WEP104,

		/// <summary>
		/// Specifies a Wi-Fi Protected Access (WPA) Use Group Key cipher suite
		/// </summary>
		WPA_USE_GROUP,

		/// <summary>
		/// Specifies a Robust Security Network (RSN) Use Group Key cipher suite
		/// </summary>
		RSN_USE_GROUP,

		/// <summary>
		/// Specifies a WEP cipher algorithm with a cipher key of any length
		/// </summary>
		WEP,

		/// <summary>
		/// Specifies the start of the range that is used to define proprietary cipher algorithms that are developed by an independent hardware vendor (IHV)
		/// </summary>
		IHV_START,

		/// <summary>
		/// Specifies the end of the range that is used to define proprietary cipher algorithms that are developed by an independent hardware vendor (IHV)
		/// </summary>
		IHV_END
	}

	internal static class CipherAlgorithmConverter
	{
		public static bool TryConvert(DOT11_CIPHER_ALGORITHM source, out CipherAlgorithm authAlgorithm)
		{
			switch (source)
			{
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_NONE:
					authAlgorithm = CipherAlgorithm.None;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WEP40:
					authAlgorithm = CipherAlgorithm.WEP40;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_TKIP:
					authAlgorithm = CipherAlgorithm.TKIP;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_CCMP:
					authAlgorithm = CipherAlgorithm.CCMP;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WEP104:
					authAlgorithm = CipherAlgorithm.WEP104;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WPA_USE_GROUP:
					authAlgorithm = CipherAlgorithm.WPA_USE_GROUP;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WEP:
					authAlgorithm = CipherAlgorithm.WEP;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_IHV_START:
					authAlgorithm = CipherAlgorithm.IHV_START;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_IHV_END:
					authAlgorithm = CipherAlgorithm.IHV_END;
					return true;
			}
			authAlgorithm = default;
			return false;
		}
	}
}
