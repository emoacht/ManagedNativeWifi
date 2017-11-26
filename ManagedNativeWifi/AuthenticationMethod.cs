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
	/// https://msdn.microsoft.com/en-us/library/windows/desktop/ms706933.aspx
	/// </remarks>
	public enum AuthenticationMethod
	{
		/// <summary>
		/// None (invalid value)
		/// </summary>
		None = 0,

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
		WPA2_Personal
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
			}
			authentication = default(AuthenticationMethod);
			return false;
		}
	}
}