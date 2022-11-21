using System;

namespace ManagedNativeWifi;

/// <summary>
/// Interface state enumeration values.
/// </summary>
public enum InterfaceChangedState
{
	/// <summary>
	/// An interface has arrived..
	/// </summary>
	Arrival,
	/// <summary>
	/// An interface has been removed.
	/// </summary>
	Removal
}

/// <summary>
/// Represents event arguments for the InterfaceChanged event.
/// </summary>
public class InterfaceChangedEventArgs : EventArgs
{
	private readonly InterfaceInfo _interfaceInfo;
	private readonly InterfaceChangedState _interfaceChangedState;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:InterfaceChangedEventArgs" /> class.
	/// </summary>
	/// <param name="interfaceInfo">An instance of the <see cref="T:InterfaceInfo" /> class</param>
	/// <param name="interfaceChangedState">One of the <see cref="T:InterfaceChangedState" /> values.</param>
	public InterfaceChangedEventArgs(InterfaceInfo interfaceInfo, InterfaceChangedState interfaceChangedState)
	{
		_interfaceInfo = interfaceInfo;
		_interfaceChangedState = interfaceChangedState;
	}

	/// <summary>
	/// Returns a value from the <see cref="T:InterfaceChangedState" /> enumerator.
	/// </summary>
	public InterfaceChangedState State => _interfaceChangedState;

	/// <summary>
	/// Returns an <see cref="T:InterfaceInfo" /> object.
	/// </summary>
	public InterfaceInfo Interface => _interfaceInfo;
}
