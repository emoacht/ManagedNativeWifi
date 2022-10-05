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
/// Profile state enumeration values.
/// </summary>
public enum EnumProfileState
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
public class ProfileArgs : EventArgs
{
	/// <summary>
	/// Returns a value from the EnumProfileState enumerator.
	/// </summary>
	public EnumProfileState State { get; set; }

	/// <summary>
	/// Returns an InterfaceInfo object.
	/// </summary>
	public InterfaceInfo Interface { get; set; }
}
