
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
	/// Only wireless interfaces that are not connected to wireless LANs, are requested to scan.
	/// </summary>
	OnlyNotConnected,

	/// <summary>
	/// Only wireless interfaces that are specified, are requested to scan.
	/// </summary>
	OnlySpecified
}