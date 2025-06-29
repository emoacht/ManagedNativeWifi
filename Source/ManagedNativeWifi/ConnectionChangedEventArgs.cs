using System;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi;

/// <summary>
/// Provides data for the ConnectionChanged event.
/// </summary>
public class ConnectionChangedEventArgs : EventArgs
{
	/// <summary>
	/// Associated wireless interface ID
	/// </summary>
	public Guid InterfaceId { get; }

	/// <summary>
	/// Wireless connection changed state
	/// </summary>
	public ConnectionChangedState ChangedState { get; }

	/// <summary>
	/// Wireless connection notification data
	/// </summary>
	public ConnectionNotificationData Data { get; }

	internal ConnectionChangedEventArgs(Guid interfaceId, ConnectionChangedState changedState, ConnectionNotificationData data)
	{
		this.InterfaceId = interfaceId;
		this.ChangedState = changedState;
		this.Data = data;
	}
}

/// <summary>
/// Wireless connection changed state
/// </summary>
public enum ConnectionChangedState
{
	/// <summary>
	/// Unknown (invalid value)
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// A connection attempt has started.
	/// </summary>
	Started,

	/// <summary>
	/// A connection has completed.
	/// </summary>
	Completed,

	/// <summary>
	/// A connection attempt has failed.
	/// </summary>
	Failed,

	/// <summary>
	/// The current connection is disconnecting.
	/// </summary>
	Disconnecting,

	/// <summary>
	/// The current connection has disconnected.
	/// </summary>
	Disconnected
}

/// <summary>
/// Wireless connection notification data
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
	public NetworkIdentifier Ssid { get; }

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
		Ssid = new NetworkIdentifier(data.dot11Ssid);

		if (BssTypeConverter.TryConvert(data.dot11BssType, out BssType bssType))
			this.BssType = bssType;

		IsSecurityEnabled = data.bSecurityEnabled;
	}
}