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
			switch (source)
			{
				case WLAN_CONNECTION_MODE.wlan_connection_mode_profile:
					return ConnectionMode.Profile;
				case WLAN_CONNECTION_MODE.wlan_connection_mode_temporary_profile:
					return ConnectionMode.TemporaryProfile;
				case WLAN_CONNECTION_MODE.wlan_connection_mode_discovery_secure:
					return ConnectionMode.DiscoverySecure;
				case WLAN_CONNECTION_MODE.wlan_connection_mode_discovery_unsecure:
					return ConnectionMode.DiscoveryUnsecure;
				case WLAN_CONNECTION_MODE.wlan_connection_mode_auto:
					return ConnectionMode.Auto;
				default:
					return ConnectionMode.Invalid;
			}
		}

		public static WLAN_CONNECTION_MODE ConvertBack(ConnectionMode source)
		{
			switch (source)
			{
				case ConnectionMode.Profile:
					return WLAN_CONNECTION_MODE.wlan_connection_mode_profile;
				case ConnectionMode.TemporaryProfile:
					return WLAN_CONNECTION_MODE.wlan_connection_mode_temporary_profile;
				case ConnectionMode.DiscoverySecure:
					return WLAN_CONNECTION_MODE.wlan_connection_mode_discovery_secure;
				case ConnectionMode.DiscoveryUnsecure:
					return WLAN_CONNECTION_MODE.wlan_connection_mode_discovery_unsecure;
				case ConnectionMode.Auto:
					return WLAN_CONNECTION_MODE.wlan_connection_mode_auto;
				default:
					return WLAN_CONNECTION_MODE.wlan_connection_mode_invalid;
			}
		}
	}
}