using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Connection mode
	/// </summary>
	/// <remarks>
	/// Equivalent to WLAN_CONNECTION_MODE:
	/// https://docs.microsoft.com/en-us/windows/win32/api/wlanapi/ne-wlanapi-wlan_connection_mode
	/// </remarks>
	public enum ConnectionMode
	{
		/// <summary>
		/// Not used.
		/// </summary>
		Invalid = 0,

		/// <summary>
		/// A profile will be used to make the connection.
		/// </summary>
		Profile,

		/// <summary>
		/// A temporary profile will be used to make the connection.
		/// </summary>
		TemporaryProfile,

		/// <summary>
		/// Secure discovery will be used to make the connection.
		/// </summary>
		DiscoverySecure,

		/// <summary>
		/// Unsecure discovery will be used to make the connection.
		/// </summary>
		DiscoveryUnsecure,

		/// <summary>
		/// The connection is initiated by the wireless service automatically using a persistent profile.
		/// </summary>
		Auto
	}

	internal static class ConnectionModeConverter
	{
		public static ConnectionMode Convert(WLAN_CONNECTION_MODE source)
		{
			return source switch
			{
				WLAN_CONNECTION_MODE.wlan_connection_mode_profile => ConnectionMode.Profile,
				WLAN_CONNECTION_MODE.wlan_connection_mode_temporary_profile => ConnectionMode.TemporaryProfile,
				WLAN_CONNECTION_MODE.wlan_connection_mode_discovery_secure => ConnectionMode.DiscoverySecure,
				WLAN_CONNECTION_MODE.wlan_connection_mode_discovery_unsecure => ConnectionMode.DiscoveryUnsecure,
				WLAN_CONNECTION_MODE.wlan_connection_mode_auto => ConnectionMode.Auto,
				_ => ConnectionMode.Invalid,
			};
		}

		public static WLAN_CONNECTION_MODE ConvertBack(ConnectionMode source)
		{
			return source switch
			{
				ConnectionMode.Profile => WLAN_CONNECTION_MODE.wlan_connection_mode_profile,
				ConnectionMode.TemporaryProfile => WLAN_CONNECTION_MODE.wlan_connection_mode_temporary_profile,
				ConnectionMode.DiscoverySecure => WLAN_CONNECTION_MODE.wlan_connection_mode_discovery_secure,
				ConnectionMode.DiscoveryUnsecure => WLAN_CONNECTION_MODE.wlan_connection_mode_discovery_unsecure,
				ConnectionMode.Auto => WLAN_CONNECTION_MODE.wlan_connection_mode_auto,
				_ => WLAN_CONNECTION_MODE.wlan_connection_mode_invalid,
			};
		}
	}
}