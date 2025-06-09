using System;

namespace ManagedNativeWifi;

/// <summary>
/// Wireless LAN information on BSS network
/// </summary>
public class BssNetworkInfo
{
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
	/// Received Signal Strength Indicator (RSSI) (dBm)
	/// </summary>
	public int Rssi { get; }

	/// <summary>
	/// Link quality (0-100)
	/// </summary>
	public int LinkQuality { get; }

	/// <summary>
	/// Channel center frequency (KHz)
	/// </summary>
	public int Frequency { get; }

	/// <summary>
	/// Frequency band (GHz)
	/// </summary>
	public float Band { get; }

	/// <summary>
	/// Channel
	/// </summary>
	public int Channel { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	public BssNetworkInfo(
		NetworkIdentifier ssid,
		BssType bssType,
		NetworkIdentifier bssid,
		PhyType phyType,
		int rssi,
		int linkQuality,
		int frequency,
		float band,
		int channel)
	{
		this.Ssid = ssid;
		this.BssType = bssType;
		this.Bssid = bssid;
		this.PhyType = phyType;
		this.Rssi = rssi;
		this.LinkQuality = linkQuality;
		this.Frequency = frequency;
		this.Band = band;
		this.Channel = channel;
	}
}

/// <summary>
/// Wireless LAN information on BSS network
/// </summary>
public class BssNetworkPack : BssNetworkInfo
{
	/// <summary>
	/// Associated wireless interface information
	/// </summary>
	public InterfaceInfo InterfaceInfo { get; }

	/// <summary>
	/// Associated wireless interface information
	/// </summary>
	[Obsolete("Use InterfaceInfo property instead.")]
	public InterfaceInfo Interface => InterfaceInfo;

	/// <summary>
	/// Constructor
	/// </summary>
	public BssNetworkPack(
		InterfaceInfo interfaceInfo,
		NetworkIdentifier ssid,
		BssType bssType,
		NetworkIdentifier bssid,
		PhyType phyType,
		int rssi,
		int linkQuality,
		int frequency,
		float band,
		int channel) : base(
			ssid: ssid,
			bssType: bssType,
			bssid: bssid,
			phyType: phyType,
			rssi: rssi,
			linkQuality: linkQuality,
			frequency: frequency,
			band: band,
			channel: channel)
	{
		this.InterfaceInfo = interfaceInfo;
	}

	internal BssNetworkPack(
		InterfaceInfo interfaceInfo,
		BssNetworkInfo bssNetworkInfo) : this(
			interfaceInfo: interfaceInfo,
			ssid: bssNetworkInfo.Ssid,
			bssType: bssNetworkInfo.BssType,
			bssid: bssNetworkInfo.Bssid,
			phyType: bssNetworkInfo.PhyType,
			rssi: bssNetworkInfo.Rssi,
			linkQuality: bssNetworkInfo.LinkQuality,
			frequency: bssNetworkInfo.Frequency,
			band: bssNetworkInfo.Band,
			channel: bssNetworkInfo.Channel)
	{ }
}