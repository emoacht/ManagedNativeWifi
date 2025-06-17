
namespace ManagedNativeWifi;

/// <summary>
/// Data encryption type to be used to connect to wireless LAN
/// </summary>
/// <remarks>
/// Equivalent to encryption element in profile XML:
/// https://learn.microsoft.com/en-us/windows/win32/nativewifi/wlan-profileschema-authencryption-security-element#encryption
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
	AES,

	/// <summary>
	/// GCMP-256 encryption for WPA3
	/// </summary>
	GCMP_256
}

internal static class EncryptionTypeConverter
{
	public static bool TryParse(string source, out EncryptionType encryption)
	{
		switch (source)
		{
			case "none":
				encryption = EncryptionType.None;
				return true;
			case "WEP":
				encryption = EncryptionType.WEP;
				return true;
			case "TKIP":
				encryption = EncryptionType.TKIP;
				return true;
			case "AES":
				encryption = EncryptionType.AES;
				return true;
			case "GCMP256":
				encryption = EncryptionType.GCMP_256;
				return true;
		}
		encryption = default;
		return false;
	}
}