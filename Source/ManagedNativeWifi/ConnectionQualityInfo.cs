using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagedNativeWifi;

/// <summary>
/// Wireless interface connection quality information
/// </summary>
public class ConnectionQualityInfo
{
	/// <summary>
	/// Interface ID
	/// </summary>
	public Guid Id { get; }

	/// <summary>
	/// Link Quality (0-100)
	/// </summary>
	public int LinkQuality { get; }

	/// <summary>
	/// Receive Rate (Kbps)
	/// </summary>
	public int RxRate { get; }

	/// <summary>
	/// Transmit Rate (Kbps)
	/// </summary>
	public int TxRate { get; }

	/// <summary>
	/// Indicates whether this is a Multi-Link Operation (MLO) connection
	/// </summary>
	public bool IsMultiLinkOperation { get; }

	/// <summary>
	/// Link information for each radio link in the connection
	/// </summary>
	public IReadOnlyList<ConnectionQualityLinkInfo> Links { get; }

	internal ConnectionQualityInfo(
		Guid id,
		int linkQuality,
		int rxRate,
		int txRate,
		bool isMultiLinkOperation,
		IEnumerable<ConnectionQualityLinkInfo> links)
	{
		Id = id;
		LinkQuality = linkQuality;
		RxRate = rxRate;
		TxRate = txRate;
		IsMultiLinkOperation = isMultiLinkOperation;
		Links = Array.AsReadOnly(links?.ToArray() ?? []);
	}
}