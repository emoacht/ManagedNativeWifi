using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Data encryption type to be used to connect to wireless LAN
	/// </summary>
	/// <remarks>
	/// https://msdn.microsoft.com/en-us/library/windows/desktop/ms706969.aspx
	/// </remarks>
	public enum EncryptionType
	{
		/// <summary>
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// WEP encryption for WEP
		/// </summary>
		WEP,

		/// <summary>
		/// TKIP encryption for WPA/WPA2
		/// </summary>
		TKIP,

		/// <summary>
		/// AES (CCMP) encryption for WPA/WPA2
		/// </summary>
		AES
	}

	internal static class EncryptionTypeConverter
	{
		public static EncryptionType ToEncryptionType(string source)
		{
			switch (source)
			{
				case "WEP":
					return EncryptionType.WEP;
				case "TKIP":
					return EncryptionType.TKIP;
				case "AES":
					return EncryptionType.AES;
				default:
					return EncryptionType.None;
			}
		}
	}
}