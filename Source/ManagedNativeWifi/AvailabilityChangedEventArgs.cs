using System;

namespace ManagedNativeWifi;

/// <summary>
/// Provides data for the AvailabilityChanged event.
/// </summary>
public class AvailabilityChangedEventArgs : EventArgs
{
	/// <summary>
	/// Associated wireless interface ID
	/// </summary>
	public Guid InterfaceId { get; }

	/// <summary>
	/// Availability changed state
	/// </summary>
	public AvailabilityChangedState ChangedState { get; }

	internal AvailabilityChangedEventArgs(Guid interfaceId, AvailabilityChangedState changedState)
	{
		this.InterfaceId = interfaceId;
		this.ChangedState = changedState;
	}
}

/// <summary>
/// Availability changed state
/// </summary>
public enum AvailabilityChangedState
{
	/// <summary>
	/// Unknown (invalid value)
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// All of the following conditions occur:
	/// A connectable wireless LAN is found after a scan.
	/// The wireless interface is in the disconnected state.
	/// There is no compatible auto-connect profile that can be used to connect.
	/// </summary>
	Available,

	/// <summary>
	/// No connectable wireless LAN is found after a scan.
	/// </summary>
	Unavailable
}