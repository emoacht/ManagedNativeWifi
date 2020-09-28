using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Authentication algorithm
	/// </summary>
	/// <remarks>
	/// Equivalent to DOT11_AUTH_ALGORITHM:
	/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/dot11-auth-algorithm
	/// </remarks>
	public enum AuthenticationAlgorithm
	{
		/// <summary>
		/// Unknown (invalid value)
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// 802.11 Open System authentication algorithm
		/// </summary>
		Open,

		/// <summary>
		/// 802.11 Shared Key authentication algorithm that uses pre-shared Wired Equivalent Privacy (WEP) key
		/// </summary>
		Shared,

		/// <summary>
		/// Wi-Fi Protected Access (WPA) algorithm
		/// </summary>
		WPA,

		/// <summary>
		/// WPA algorithm that uses pre-shared keys (PSK)
		/// </summary>
		WPA_PSK,

		/// <summary>
		/// Note supported
		/// </summary>
		WPA_NONE,

		/// <summary>
		/// 802.11i Robust Security Network Association (RSNA) algorithm (WPA2 is one such algorithm.)
		/// </summary>
		RSNA,

		/// <summary>
		/// 802.11i RSNA algorithm that uses PSK
		/// </summary>
		RSNA_PSK,

		/// <summary>
		/// Indicates the start of the range that specifies proprietary authentication algorithms developed by an independent hardware vendor (IHV).
		/// </summary>
		IHV_START,

		/// <summary>
		/// Indicates the end of the range that specifies proprietary authentication algorithms developed by an independent hardware vendor (IHV).
		/// </summary>
		IHV_END
	}

	internal static class AuthenticationAlgorithmConverter
	{
		public static bool TryConvert(DOT11_AUTH_ALGORITHM source, out AuthenticationAlgorithm authenticationAlgorithm)
		{
			switch (source)
			{
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_80211_OPEN:
					authenticationAlgorithm = AuthenticationAlgorithm.Open;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_80211_SHARED_KEY:
					authenticationAlgorithm = AuthenticationAlgorithm.Shared;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_WPA:
					authenticationAlgorithm = AuthenticationAlgorithm.WPA;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_WPA_PSK:
					authenticationAlgorithm = AuthenticationAlgorithm.WPA_PSK;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_WPA_NONE:
					authenticationAlgorithm = AuthenticationAlgorithm.WPA_NONE;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_RSNA:
					authenticationAlgorithm = AuthenticationAlgorithm.RSNA;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_RSNA_PSK:
					authenticationAlgorithm = AuthenticationAlgorithm.RSNA_PSK;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_IHV_START:
					authenticationAlgorithm = AuthenticationAlgorithm.IHV_START;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_IHV_END:
					authenticationAlgorithm = AuthenticationAlgorithm.IHV_END;
					return true;
			}
			authenticationAlgorithm = default;
			return false;
		}
	}
}