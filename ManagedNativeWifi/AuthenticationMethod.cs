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
		/// None
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
		public static AuthenticationMethod ToAuthenticationMethod(string source)
		{
			switch (source)
			{
				case "open":
					return AuthenticationMethod.Open;
				case "shared":
					return AuthenticationMethod.Shared;
				case "WPA":
					return AuthenticationMethod.WPA_Enterprise;
				case "WPAPSK":
					return AuthenticationMethod.WPA_Personal;
				case "WPA2":
					return AuthenticationMethod.WPA2_Enterprise;
				case "WPA2PSK":
					return AuthenticationMethod.WPA2_Personal;
				default:
					return AuthenticationMethod.None;
			}
		}
	}
}