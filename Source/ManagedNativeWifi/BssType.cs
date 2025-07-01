﻿using System;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi;

/// <summary>
/// BSS network type
/// </summary>
/// <remarks>
/// Partly equivalent to DOT11_BSS_TYPE:
/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/dot11-bss-type
/// Also equivalent to connectionType element in profile XML:
/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/wlan-profileschema-connectiontype-wlanprofile-element
/// </remarks>
public enum BssType
{
	/// <summary>
	/// None (invalid value)
	/// </summary>
	None = 0,

	/// <summary>
	/// Infrastructure BSS network
	/// </summary>
	Infrastructure,

	/// <summary>
	/// Independent BSS (IBSS) network (Ad hoc network)
	/// </summary>
	Independent,

	/// <summary>
	/// Either infrastructure or independent BSS network
	/// </summary>
	Any
}

internal static class BssTypeConverter
{
	/// <summary>
	/// Converts from DOT11_BSS_TYPE value.
	/// </summary>
	/// <param name="source">DOT11_BSS_TYPE value</param>
	/// <param name="bssType">BssType value</param>
	/// <returns>True if successfully converts</returns>
	public static bool TryConvert(DOT11_BSS_TYPE source, out BssType bssType)
	{
		switch (source)
		{
			case DOT11_BSS_TYPE.dot11_BSS_type_infrastructure:
				bssType = BssType.Infrastructure;
				return true;
			case DOT11_BSS_TYPE.dot11_BSS_type_independent:
				bssType = BssType.Independent;
				return true;
			case DOT11_BSS_TYPE.dot11_BSS_type_any:
				bssType = BssType.Any;
				return true;
		}
		bssType = default;
		return false;
	}

	public static DOT11_BSS_TYPE ConvertBack(BssType source)
	{
		return source switch
		{
			BssType.Infrastructure => DOT11_BSS_TYPE.dot11_BSS_type_infrastructure,
			BssType.Independent => DOT11_BSS_TYPE.dot11_BSS_type_independent,
			BssType.Any => DOT11_BSS_TYPE.dot11_BSS_type_any,
			_ => default // Non-defined invalid value
		};
	}

	/// <summary>
	/// Converts from connectionType element in profile XML.
	/// </summary>
	/// <param name="source">connectionType element</param>
	/// <param name="bssType">BssType value</param>
	/// <returns>True if successfully converts</returns>
	public static bool TryParse(string source, out BssType bssType)
	{
		if (string.Equals("ESS", source, StringComparison.OrdinalIgnoreCase))
		{
			bssType = BssType.Infrastructure;
			return true;
		}
		if (string.Equals("IBSS", source, StringComparison.OrdinalIgnoreCase))
		{
			bssType = BssType.Independent;
			return true;
		}
		bssType = default;
		return false;
	}
}