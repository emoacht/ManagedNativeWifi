using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
    public enum AuthType
    {
		Open = 0,
		Shared,
		WPA,
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
