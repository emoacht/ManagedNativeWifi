
using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi;

/// <summary>
/// Radio state information
/// </summary>
/// <remarks>
/// Partly equivalent to WLAN_PHY_RADIO_STATE:
/// https://learn.microsoft.com/en-us/windows/win32/api/wlanapi/ns-wlanapi-wlan_phy_radio_state
/// </remarks>
public class RadioStateSet
{
	/// <summary>
	/// PHY type
	/// </summary>
	public PhyType PhyType { get; }

	/// <summary>
	/// Whether hardware radio state is on
	/// </summary>
	public bool IsHardwareOn { get; }

	/// <summary>
	/// Whether software radio state is on
	/// </summary>
	public bool IsSoftwareOn { get; }

	/// <summary>
	/// Whether radio state is on
	/// </summary>
	public bool IsOn => IsHardwareOn && IsSoftwareOn;

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="phyType">PHY type</param>
	/// <param name="isHardwareOn">Whether hardware state is on</param>
	/// <param name="isSoftwareOn">Whether software state is on</param>
	public RadioStateSet(PhyType phyType, bool isHardwareOn, bool isSoftwareOn)
	{
		this.PhyType = phyType;
		this.IsHardwareOn = isHardwareOn;
		this.IsSoftwareOn = isSoftwareOn;
	}

	internal RadioStateSet(PhyType phyType, WLAN_PHY_RADIO_STATE state)
	{
		this.PhyType = phyType;
		IsHardwareOn = state.dot11HardwareRadioState is DOT11_RADIO_STATE.dot11_radio_state_on;
		IsSoftwareOn = state.dot11SoftwareRadioState is DOT11_RADIO_STATE.dot11_radio_state_on;
	}
}