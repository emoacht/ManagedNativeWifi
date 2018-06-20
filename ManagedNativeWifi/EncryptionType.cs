using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ManagedNativeWifi.Win32.NativeMethod;

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
		/// None (valid value)
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
		public static bool TryParse(string source, out EncryptionType encryption)
		{
			switch (source)
			{
				case "WEP":
					encryption = EncryptionType.WEP;
					return true;
				case "TKIP":
					encryption = EncryptionType.TKIP;
					return true;
				case "AES":
					encryption = EncryptionType.AES;
					return true;
				case "none":
					encryption = EncryptionType.None;
					return true;
			}
			encryption = default(EncryptionType);
			return false;
		}

		public static bool TryConvert(DOT11_CIPHER_ALGORITHM source, out EncryptionType encryption)
		{
			switch (source)
			{
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_NONE:
					encryption = EncryptionType.None;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WEP40:
					encryption = EncryptionType.WEP;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_TKIP:
					encryption = EncryptionType.TKIP;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_CCMP:
					encryption = EncryptionType.AES;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WEP104:
					encryption = EncryptionType.WEP;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WPA_USE_GROUP:
					encryption = EncryptionType.None;
					return true;
				case DOT11_CIPHER_ALGORITHM.DOT11_CIPHER_ALGO_WEP:
					encryption = EncryptionType.WEP;
					return true;
			}
			encryption = default(EncryptionType);
			return false;
        }
	}
}