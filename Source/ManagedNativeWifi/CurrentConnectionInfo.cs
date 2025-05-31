using System;

namespace ManagedNativeWifi;

/// <summary>
/// Current connection information
/// </summary>
/// <remarks>
/// Partly equivalent to WLAN_CONNECTION_ATTRIBUTES:
/// https://learn.microsoft.com/en-us/windows/win32/api/wlanapi/ns-wlanapi-wlan_connection_attributes
/// </remarks>
public class CurrentConnectionInfo
{
	/// <summary>
	/// Interface ID
	/// </summary>
	public Guid InterfaceId { get; }

	/// <summary>
	/// Interface state
	/// </summary>
	public InterfaceState InterfaceState { get; }

	/// <summary>
	/// Connection mode
	/// </summary>
	public ConnectionMode ConnectionMode { get; }

	/// <summary>
	/// Wireless profile name
	/// </summary>
	public string ProfileName { get; }

	/// <summary>
	/// SSID (maximum 32 bytes) 
	/// </summary>
	public NetworkIdentifier Ssid { get; }

	/// <summary>
	/// BSS network type
	/// </summary>
	public BssType BssType { get; }

	/// <summary>
	/// BSSID (6 bytes)
	/// </summary>
	public NetworkIdentifier Bssid { get; }

	/// <summary>
	/// PHY type
	/// </summary>
	public PhyType PhyType { get; }

	/// <summary>
	/// The index of the PHY type
	/// </summary>
	public uint PhyIndex { get; }

	/// <summary>
	/// Signal quality (0-100)
	/// </summary>
	public int SignalQuality { get; }

	/// <summary>
	/// Receiving rate (Kbps)
	/// </summary>
	public int RxRate { get; }

	/// <summary>
	/// Transmission rate (Kbps)
	/// </summary>
	public int TxRate { get; }

	/// <summary>
	/// Whether security is enabled for this connection
	/// </summary>
	public bool IsSecurityEnabled { get; }

	/// <summary>
	/// Whether 802.1X is enabled for this connection
	/// </summary>
	public bool IsOneXEnabled { get; }

	/// <summary>
	/// Authentication algorithm
	/// </summary>
	public AuthenticationAlgorithm AuthenticationAlgorithm { get; }

	/// <summary>
	/// Cipher algorithm
	/// </summary>
	public CipherAlgorithm CipherAlgorithm { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	internal CurrentConnectionInfo(
		Guid interfaceId,
		InterfaceState interfaceState,
		ConnectionMode connectionMode,
		string profileName,
		NetworkIdentifier ssid,
		BssType bssType,
		NetworkIdentifier bssid,
		PhyType phyType,
		uint phyIndex,
		int signalQuality,
		int rxRate,
		int txRate,
		bool isSecurityEnabled,
		bool isOneXEnabled,
		AuthenticationAlgorithm authenticationAlgorithm,
		CipherAlgorithm cipherAlgorithm)
	{
		this.InterfaceId = interfaceId;
		this.InterfaceState = interfaceState;
		this.ConnectionMode = connectionMode;
		this.ProfileName = profileName;
		this.Ssid = ssid;
		this.BssType = bssType;
		this.Bssid = bssid;
		this.PhyType = phyType;
		this.PhyIndex = phyIndex;
		this.SignalQuality = signalQuality;
		this.RxRate = rxRate;
		this.TxRate = txRate;
		this.IsSecurityEnabled = isSecurityEnabled;
		this.IsOneXEnabled = isOneXEnabled;
		this.AuthenticationAlgorithm = authenticationAlgorithm;
		this.CipherAlgorithm = cipherAlgorithm;
	}
}