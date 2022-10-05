/*
 * 2022/10/04 : Added by C. Pohlmann 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi.Args;

/// <summary>
/// Availability enumeration values.
/// </summary>
public enum EnumAvailability
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
public class AvailabilityArgs : EventArgs
{
	/// <summary>
	/// Returns a value from the EnumAvailability enumerator.
	/// </summary>
	public EnumAvailability State { get; set; }

	/// <summary>
	/// Returns an InterfaceInfo object.
	/// </summary>
	public InterfaceInfo Interface { get; set; }
}
