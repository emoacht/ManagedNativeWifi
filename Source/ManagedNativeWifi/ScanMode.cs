
namespace ManagedNativeWifi;

/// <summary>
/// Modes to scan wireless LANs
/// </summary>
public enum ScanMode
{
	/// <summary>
	/// Default
	/// </summary>
	None = 0,

	/// <summary>
	/// All wireless interfaces are requested to scan.
	/// </summary>
	All,

	/// <summary>
	/// Only disconnected wireless interfaces are requested to scan.
	/// </summary>
	OnlyDisconnected,

	/// <summary>
	/// Only specified wireless interfaces are requested to scan.
	/// </summary>
	OnlySpecified
}