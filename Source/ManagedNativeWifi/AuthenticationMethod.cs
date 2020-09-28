using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Authentication method to be used to connect to wireless LAN
	/// </summary>
	/// <remarks>
	/// Equivalent to authentication element in profile XML:
	/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/wlan-profileschema-authentication-authencryption-element
	/// WPA3 values are found in:
	/// https://docs.microsoft.com/en-us/uwp/api/windows.networking.connectivity.networkauthenticationtype
	/// </remarks>
	public enum AuthenticationMethod
	{
		/// <summary>
		/// Unknown (invalid value)
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Open 802.11 authentication
		/// </summary>
		Open,

		/// <summary>
		/// Shared 802.11 authentication
		/// </summary>
		Shared,

		/// <summary>
		/// WPA-Enterprise 802.11 authentication
		/// </summary>
		/// <remarks>WPA in profile XML</remarks>
		WPA_Enterprise,

		/// <summary>
		/// WPA-Personal 802.11 authentication
		/// </summary>
		/// <remarks>WPAPSK in profile XML</remarks>
		WPA_Personal,

		/// <summary>
		/// WPA2-Enterprise 802.11 authentication
		/// </summary>
		/// <remarks>WPA2 in profile XML</remarks>
		WPA2_Enterprise,

		/// <summary>
		/// WPA2-Personal 802.11 authentication
		/// </summary>
		/// <remarks>WPA2PSK in profile XML</remarks>
		WPA2_Personal,

		/// <summary>
		/// WPA3-Enterprise 802.11 authentication
		/// </summary>
		/// <remarks>WPA3 in profile XML</remarks>
		WPA3_Enterprise,

		/// <summary>
		/// WPA3-Personal 802.11 authentication
		/// </summary>
		/// <remarks>WPA3SAE in profile XML</remarks>
		WPA3_Personal
	}

	internal static class AuthenticationMethodConverter
	{
		public static bool TryParse(string source, out AuthenticationMethod authentication)
		{
			switch (source)
			{
				case "open":
					authentication = AuthenticationMethod.Open;
					return true;
				case "shared":
					authentication = AuthenticationMethod.Shared;
					return true;
				case "WPA":
					authentication = AuthenticationMethod.WPA_Enterprise;
					return true;
				case "WPAPSK":
					authentication = AuthenticationMethod.WPA_Personal;
					return true;
				case "WPA2":
					authentication = AuthenticationMethod.WPA2_Enterprise;
					return true;
				case "WPA2PSK":
					authentication = AuthenticationMethod.WPA2_Personal;
					return true;
				case "WPA3":
					authentication = AuthenticationMethod.WPA3_Enterprise;
					return true;
				case "WPA3SAE":
					authentication = AuthenticationMethod.WPA3_Personal;
					return true;
			}
			authentication = default;
			return false;
		}
	}
}