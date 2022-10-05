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
/// Interface state enumeration values.
/// </summary>
public enum EnumInterfaceState
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
public class InterfaceArgs : EventArgs
{
	/// <summary>
	/// Returns a value from the EnumInterfaceState enumerator.
	/// </summary>
	public EnumInterfaceState State { get; set; }

	/// <summary>
	/// Returns an InterfaceInfo object.
	/// </summary>
	public InterfaceInfo Interface { get; set; }
}
