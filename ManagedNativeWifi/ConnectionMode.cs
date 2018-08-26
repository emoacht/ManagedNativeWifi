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
	/// <remarks>Equivalent to WLAN_CONNECTION_MODE</remarks>
	public enum ConnectionMode
	{
		/// <summary>
		/// A profile will be used to make the connection.
		/// </summary>
		Profile = 0,

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
		Auto,

		/// <summary>
		/// Not used.
		/// </summary>
		Invalid
	}

	internal static class ConnectionModeConverter
	{
		public static ConnectionMode Convert(WLAN_CONNECTION_MODE source) =>
			(ConnectionMode)source;

		public static WLAN_CONNECTION_MODE ConvertBack(ConnectionMode source) =>
			(WLAN_CONNECTION_MODE)source;
	}
}