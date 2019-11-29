using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Enumerated type defines a wireless LAN authentication algorithm
	/// </summary>
	public enum AuthAlgorithm
	{
		/// <summary>
		/// Specifies an IEEE 802.11 Open System authentication algorithm
		/// </summary>
		Open,

		/// <summary>
		/// Specifies an 802.11 Shared Key authentication algorithm that requires the use of a pre-shared Wired Equivalent Privacy (WEP) key for the 802.11 authentication
		/// </summary>
		SharedKey,

		/// <summary>
		/// Specifies a Wi-Fi Protected Access (WPA) algorithm
		/// </summary>
		WPA,

		/// <summary>
		/// Specifies a WPA algorithm that uses preshared keys (PSK)
		/// </summary>
		WPA_PSK,

		/// <summary>
		/// This value is not supported
		/// </summary>
		WPA_NONE,

		/// <summary>
		/// Specifies an 802.11i Robust Security Network Association (RSNA) algorithm. WPA2 is one such algorithm
		/// </summary>
		RSNA,

		/// <summary>
		/// Specifies an 802.11i RSNA algorithm that uses PSK
		/// </summary>
		RSNA_PSK,

		/// <summary>
		/// Indicates the start of the range that specifies proprietary authentication algorithms that are developed by an independent hardware vendor (IHV)
		/// </summary>
		IHV_START,

		/// <summary>
		/// Indicates the end of the range that specifies proprietary authentication algorithms that are developed by an independent hardware vendor (IHV)
		/// </summary>
		IHV_END
	}

	internal static class AuthAlgorithmConverter
	{
		public static bool TryConvert(DOT11_AUTH_ALGORITHM source, out AuthAlgorithm authAlgorithm)
		{
			switch (source)
			{
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_80211_OPEN:
					authAlgorithm = AuthAlgorithm.Open;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_80211_SHARED_KEY:
					authAlgorithm = AuthAlgorithm.SharedKey;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_WPA:
					authAlgorithm = AuthAlgorithm.WPA;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_WPA_PSK:
					authAlgorithm = AuthAlgorithm.WPA_PSK;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_WPA_NONE:
					authAlgorithm = AuthAlgorithm.WPA_NONE;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_RSNA:
					authAlgorithm = AuthAlgorithm.RSNA;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_RSNA_PSK:
					authAlgorithm = AuthAlgorithm.RSNA_PSK;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_IHV_START:
					authAlgorithm = AuthAlgorithm.IHV_START;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_IHV_END:
					authAlgorithm = AuthAlgorithm.IHV_END;
					return true;
			}
			authAlgorithm = default;
			return false;
		}
	}
}