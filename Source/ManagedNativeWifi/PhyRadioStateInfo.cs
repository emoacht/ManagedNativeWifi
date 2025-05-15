
using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi;

/// <summary>
/// Wireless radio information
/// </summary>
public class PhyRadioStateInfo
{
	/// <summary>
	/// The index of the PHY type on which the radio state is being set or queried
	/// </summary>
	public uint PhyIndex { get; }

	/// <summary>
	/// Whether hardware radio state is on
	/// </summary>
	public bool HardwareOn { get; }

	/// <summary>
	/// Whether software radio state is on
	/// </summary>
	public bool SoftwareOn { get; }

	internal PhyRadioStateInfo(WLAN_PHY_RADIO_STATE data)
	{
		PhyIndex = data.dwPhyIndex;
		HardwareOn = data.dot11HardwareRadioState is DOT11_RADIO_STATE.dot11_radio_state_on;
		SoftwareOn = data.dot11SoftwareRadioState is DOT11_RADIO_STATE.dot11_radio_state_on;
	}
}