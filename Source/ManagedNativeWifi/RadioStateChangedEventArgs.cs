using System;

namespace ManagedNativeWifi;

/// <summary>
/// Provides data for the RadioStateChanged event.
/// </summary>
public class RadioStateChangedEventArgs : EventArgs
{
	/// <summary>
	/// Associated wireless interface ID
	/// </summary>
	public Guid InterfaceId { get; }

	/// <summary>
	/// PHY radio state information
	/// </summary>
	public PhyRadioStateInfo PhyRadioStateInfo { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RadioStateChangedEventArgs"/> class.
	/// </summary>
	/// <param name="interfaceId">Interface ID</param>
	/// <param name="phyRadioStateInfo">PHY radio state information</param>
	public RadioStateChangedEventArgs(Guid interfaceId, PhyRadioStateInfo phyRadioStateInfo)
	{
		InterfaceId = interfaceId;
		PhyRadioStateInfo = phyRadioStateInfo;
	}
}