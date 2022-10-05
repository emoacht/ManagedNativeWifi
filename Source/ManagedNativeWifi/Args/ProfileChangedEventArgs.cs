using System;

namespace ManagedNativeWifi;

/// <summary>
/// Profile state enumeration values.
/// </summary>
public enum ProfileState
{
	/// <summary>
	/// A profile has changed.
	/// </summary>
	Change,
	/// <summary>
	/// A profile name has changed.
	/// </summary>
	NameChange,
	/// <summary>
	/// A profile has been unblocked.
	/// </summary>
	Unblocked,
	/// <summary>
	/// A profile has been blocked.
	/// </summary>
	Blocked
}

/// <summary>
/// Represents event arguments for the ProfileChanged event.
/// </summary>
public class ProfileChangedEventArgs : EventArgs
{
	private readonly InterfaceInfo _interfaceInfo;
	private readonly ProfileState _profileState;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ProfileChangedEventArgs" /> class.
	/// </summary>
	/// <param name="interfaceInfo">An instance of the <see cref="T:InterfaceInfo" /> class</param>
	/// <param name="profileState">One of the <see cref="T:ProfileState" /> values.</param>
	public ProfileChangedEventArgs(InterfaceInfo interfaceInfo, ProfileState profileState)
	{
		_interfaceInfo = interfaceInfo;
		_profileState = profileState;
	}

	/// <summary>
	/// Returns a value from the <see cref="T:ProfileState" /> enumerator.
	/// </summary>
	public ProfileState State => _profileState;

	/// <summary>
	/// Returns an <see cref="T:InterfaceInfo" /> object.
	/// </summary>
	public InterfaceInfo Interface => _interfaceInfo;
}
