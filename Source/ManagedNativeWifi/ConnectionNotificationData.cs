using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Connection notification data
	/// </summary>
	/// <remarks>
	/// Partly equivalent to WLAN_CONNECTION_NOTIFICATION_DATA structure:
	/// https://learn.microsoft.com/en-us/windows/win32/api/wlanapi/ns-wlanapi-wlan_connection_notification_data
	/// </remarks>
	public class ConnectionNotificationData
	{
		/// <summary>
		/// Connection mode
		/// </summary>
		public ConnectionMode ConnectionMode { get; }

		/// <summary>
		/// Associated wireless profile name
		/// </summary>
		public string ProfileName { get; }

		/// <summary>
		/// SSID of associated wireless LAN
		/// </summary>
		public NetworkIdentifier SSID { get; }

		/// <summary>
		/// BSS network type of associated wireless LAN
		/// </summary>
		public BssType BssType { get; }

		/// <summary>
		/// Whether security is enabled on this network
		/// </summary>
		public bool IsSecurityEnabled { get; }

		internal ConnectionNotificationData(WLAN_CONNECTION_NOTIFICATION_DATA data)
		{
			ConnectionMode = ConnectionModeConverter.Convert(data.wlanConnectionMode);
			ProfileName = data.strProfileName;
			SSID = new NetworkIdentifier(data.dot11Ssid);

			if (BssTypeConverter.TryConvert(data.dot11BssType, out BssType bssType))
				this.BssType = bssType;

			IsSecurityEnabled = data.bSecurityEnabled;
		}
	}
}