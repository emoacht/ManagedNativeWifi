namespace ManagedNativeWifi;

/// <summary>
/// Information about an individual radio link in a wireless connection
/// </summary>
public class ConnectionQualityLinkInfo
{
	/// <summary>
	/// Link ID
	/// </summary>
	public byte LinkId { get; }

	/// <summary>
	/// Received Signal Strength Indicator (dBm)
	/// </summary>
	public int Rssi { get; }

	/// <summary>
	/// Channel Center frequency (KHz)
	/// </summary>
	public int Frequency { get; }

	/// <summary>
	/// Bandwidth (MHz)
	/// </summary>
	public int Bandwidth { get; }

	internal ConnectionQualityLinkInfo(
		byte linkId,
		int rssi,
		int frequency,
		int bandwidth)
	{
		LinkId = linkId;
		Rssi = rssi;
		Frequency = frequency;
		Bandwidth = bandwidth;
	}
}