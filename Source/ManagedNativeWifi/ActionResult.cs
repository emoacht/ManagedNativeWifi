
namespace ManagedNativeWifi;

/// <summary>
/// Action result
/// </summary>
public enum ActionResult
{
	/// <summary>
	/// None
	/// </summary>
	None = 0,

	/// <summary>
	/// The action succeeded.
	/// </summary>
	Success,

	/// <summary>
	/// The action failed because the interface was not connected to a wireless LAN.
	/// </summary>
	NotConnected,

	/// <summary>
	/// The action failed because the interface was not found.
	/// </summary>
	NotFound,

	/// <summary>
	/// The action failed because the function was not supported.
	/// </summary>
	NotSupported,

	/// <summary>
	/// The action failed due to other error.
	/// </summary>
	OtherError
}