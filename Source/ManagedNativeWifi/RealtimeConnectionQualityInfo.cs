using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagedNativeWifi;

/// <summary>
/// Real-time wireless connection quality information
/// </summary>
/// <remarks>
/// Partly equivalent to WLAN_REALTIME_CONNECTION_QUALITY:
/// https://learn.microsoft.com/en-us/windows/win32/api/wlanapi/ns-wlanapi-wlan_realtime_connection_quality
/// </remarks>
public class RealtimeConnectionQualityInfo
{
	/// <summary>
	/// PHY type
	/// </summary>
	public PhyType PhyType { get; }

	/// <summary>
	/// Link quality (0-100)
	/// </summary>
	public int LinkQuality { get; }

	/// <summary>
	/// Receiving rate (Kbps)
	/// </summary>
	public int RxRate { get; }

	/// <summary>
	/// Transmission rate (Kbps)
	/// </summary>
	public int TxRate { get; }

	/// <summary>
	/// Whether this is a Multi-Link Operation (MLO) connection
	/// </summary>
	public bool IsMultiLinkOperation { get; }

	/// <summary>
	/// Link information for radio links in this connection
	/// </summary>
	public IReadOnlyList<RealtimeConnectionQualityLinkInfo> Links { get; }

	internal RealtimeConnectionQualityInfo(
		PhyType phyType,
		int linkQuality,
		int rxRate,
		int txRate,
		bool isMultiLinkOperation,
		IEnumerable<RealtimeConnectionQualityLinkInfo> links)
	{
		this.PhyType = phyType;
		this.LinkQuality = linkQuality;
		this.RxRate = rxRate;
		this.TxRate = txRate;
		this.IsMultiLinkOperation = isMultiLinkOperation;
		this.Links = Array.AsReadOnly(links?.ToArray() ?? []);
	}
}

/// <summary>
/// Link information for a radio link in real-time wireless connection quality information
/// </summary>
public class RealtimeConnectionQualityLinkInfo
{
	/// <summary>
	/// Link ID
	/// </summary>
	public byte LinkId { get; }

	/// <summary>
	/// Received Signal Strength Indicator (RSSI) (dBm)
	/// </summary>
	public int Rssi { get; }

	/// <summary>
	/// Channel center frequency (MHz)
	/// </summary>
	public int Frequency { get; }

	/// <summary>
	/// Bandwidth (MHz)
	/// </summary>
	public int Bandwidth { get; }

	internal RealtimeConnectionQualityLinkInfo(
		byte linkId,
		int rssi,
		int frequency,
		int bandwidth)
	{
		this.LinkId = linkId;
		this.Rssi = rssi;
		this.Frequency = frequency;
		this.Bandwidth = bandwidth;
	}
}