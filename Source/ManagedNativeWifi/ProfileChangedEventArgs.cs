using System;

namespace ManagedNativeWifi;

/// <summary>
/// Provides data for the ProfileChanged event.
/// </summary>
public class ProfileChangedEventArgs : EventArgs
{
	/// <summary>
	/// Associated wireless interface ID
	/// </summary>
	public Guid InterfaceId { get; }

	/// <summary>
	/// Profile changed state
	/// </summary>
	public ProfileChangedState ChangedState { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ProfileChangedEventArgs"/> class.
	/// </summary>
	/// <param name="interfaceId">Interface ID</param>
	/// <param name="changedState">Profile changed state</param>
	public ProfileChangedEventArgs(Guid interfaceId, ProfileChangedState changedState)
	{
		this.InterfaceId = interfaceId;
		this.ChangedState = changedState;
	}
}

/// <summary>
/// Wireless profile changed state
/// </summary>
public enum ProfileChangedState
{
	/// <summary>
	/// Unknown (invalid value)
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// A change in a profile or the profile list has occurred.
	/// </summary>
	Changed,

	/// <summary>
	/// A profile name has changed.
	/// </summary>
	NameChanged,

	/// <summary>
	/// A profile has been unblocked.
	/// </summary>
	Unblocked,

	/// <summary>
	/// A profile has been blocked.
	/// </summary>
	Blocked
}