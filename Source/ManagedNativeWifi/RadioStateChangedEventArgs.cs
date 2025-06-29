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

	/// <summary>
	/// Initializes a new instance of the <see cref="RadioStateChangedEventArgs"/> class.
	/// </summary>
	/// <param name="interfaceId">Interface ID</param>
	/// <param name="radioState">Radio state information</param>
	public RadioStateChangedEventArgs(Guid interfaceId, RadioStateSet radioState)
	{
		this.InterfaceId = interfaceId;
		this.RadioState = radioState;
	}
}