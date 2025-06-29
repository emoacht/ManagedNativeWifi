using System;

namespace ManagedNativeWifi;

/// <summary>
/// Provides data for the InterfaceChanged event.
/// </summary>
public class InterfaceChangedEventArgs : EventArgs
{
	/// <summary>
	/// Associated wireless interface ID
	/// </summary>
	public Guid InterfaceId { get; }

	/// <summary>
	/// Interface changed state
	/// </summary>
	public InterfaceChangedState ChangedState { get; }

	internal InterfaceChangedEventArgs(Guid interfaceId, InterfaceChangedState changedState)
	{
		this.InterfaceId = interfaceId;
		this.ChangedState = changedState;
	}
}

/// <summary>
/// Wireless interface changed state
/// </summary>
public enum InterfaceChangedState
{
	/// <summary>
	/// Unknown (invalid value)
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// A wireless interface has been added to or enabled.
	/// </summary>
	Arrived,

	/// <summary>
	/// A wireless interface has been removed or disabled.
	/// </summary>
	Removed
}