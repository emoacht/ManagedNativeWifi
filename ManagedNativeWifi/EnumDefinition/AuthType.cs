using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
    /// <summary>
    /// Defines a wireless LAN authentication algorithm. (<see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms705989(v=vs.85).aspx">Doument</see>)
    /// </summary>
    /// <see cref="DOT11_AUTH_ALGORITHM"/>
    public enum AuthType
    {
        /// <summary>
        /// Specifies an IEEE 802.11 Open System authentication algorithm.
        /// </summary>
        Open = 0,
        /// <summary>
        /// Specifies an 802.11 Shared Key authentication algorithm that requires the use of a pre-shared Wired Equivalent Privacy (WEP) key for the 802.11 authentication.
        /// </summary>
        Shared,
        /// <summary>
	    /// Specifies a Wi-Fi Protected Access (WPA) algorithm. IEEE 802.1X port authentication is performed by the supplicant, authenticator, and authentication server. Cipher keys are dynamically derived through the authentication process.
	    /// This algorithm is valid only for BSS types of dot11_BSS_type_infrastructure.
	    /// When the WPA algorithm is enabled, the 802.11 station will associate only with an access point whose beacon or probe responses contain the authentication suite of type 1 (802.1X) within the WPA information element (IE).
        /// </summary>
		WPA,
        /// <summary>
	    /// Specifies an 802.11i Robust Security Network Association (RSNA) algorithm. WPA2 is one such algorithm. IEEE 802.1X port authentication is performed by the supplicant, authenticator, and authentication server. Cipher keys are dynamically derived through the authentication process.
	    /// This algorithm is valid only for BSS types of dot11_BSS_type_infrastructure.
	    /// When the RSNA algorithm is enabled, the 802.11 station will associate only with an access point whose beacon or probe responses contain the authentication suite of type 1 (802.1X) within the RSN IE.
        /// </summary>
		WPA2

		
    }

	internal static class AuthTypeConverter
	{
		public static bool TryParse(string source, out AuthType auth)
		{
			switch (source)
			{
				case "Open":
					auth = AuthType.Open;
					return true;
				case "Shared":
					auth = AuthType.Shared;
					return true;
				case "WPA":
					auth = AuthType.WPA;
					return true;
				case "WPA2":
					auth = AuthType.WPA2;
					return true;
			}
			auth = default(AuthType);
			return false;
		}

		public static bool TryConvert(DOT11_AUTH_ALGORITHM source, out AuthType auth)
		{
			switch (source)
			{
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_80211_OPEN:
					auth = AuthType.Open;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_80211_SHARED_KEY:
					auth = AuthType.Shared;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_WPA:
					auth = AuthType.WPA;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_WPA_PSK:
					auth = AuthType.WPA;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_WPA_NONE:
					auth = AuthType.WPA;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_RSNA:
					auth = AuthType.WPA2;
					return true;
				case DOT11_AUTH_ALGORITHM.DOT11_AUTH_ALGO_RSNA_PSK:
					auth = AuthType.WPA2;
					return true;
			}

			auth = default(AuthType);
			return false;
		}
	}
}
