using System;

namespace ManagedNativeWifi;

/// <summary>
/// Availability enumeration values.
/// </summary>
public enum Availability
{
	Start,
	End,
	/// <summary>
	/// Network is available.
	/// </summary>
	Available,
	/// <summary>
	/// Network is not available.
	/// </summary>
	Unavailable
}

/// <summary>
/// Represents event arguments for the AvailabilityChanged event.
/// </summary>
public class AvailabilityChangedEventArgs : EventArgs
{
	private readonly InterfaceInfo _interfaceInfo;
	private readonly Availability _availability;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:AvailabilityChangedEventArgs" /> class.
	/// </summary>
	/// <param name="interfaceInfo">An instance of the <see cref="T:InterfaceInfo" /> class</param>
	/// <param name="availability">One of the <see cref="T:Availability" /> values.</param>
	public AvailabilityChangedEventArgs(InterfaceInfo interfaceInfo, Availability availability)
	{
		_interfaceInfo = interfaceInfo;
		_availability = availability;
	}

	/// <summary>
	/// Returns a value from the Availability enumerator.
	/// </summary>
	public Availability State => _availability;

	/// <summary>
	/// Returns an InterfaceInfo object.
	/// </summary>
	public InterfaceInfo Interface => _interfaceInfo;
}
