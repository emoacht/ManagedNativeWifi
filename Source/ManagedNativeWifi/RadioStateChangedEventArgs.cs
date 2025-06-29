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
	/// Radio state information
	/// </summary>
	public RadioStateSet RadioState { get; }

	internal RadioStateChangedEventArgs(Guid interfaceId, RadioStateSet radioState)
	{
		this.InterfaceId = interfaceId;
		this.RadioState = radioState;
	}
}