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
/// Connection state enumeration values.
/// </summary>
public enum EnumConnectionState
{
	/// <summary>
	/// A connection attempt has started.
	/// </summary>
	Start,
	/// <summary>
	/// A connection attempt is complete.
	/// </summary>
	Complete,
	/// <summary>
	/// A connection attempt failed.
	/// </summary>
	Failed,
	/// <summary>
	/// The current connection is disconnecting.
	/// </summary>
	Disconnecting,
	/// <summary>
	/// The current connection is disconnected.
	/// </summary>
	Disconnected
}

/// <summary>
/// Represents event arguments for the ConnectionChanged event.
/// </summary>
public class ConnectionArgs : EventArgs
{
	/// <summary>
	/// Returns a value from the EnumConnectionState enumerator.
	/// </summary>
	public EnumConnectionState State { get; set; }

	/// <summary>
	/// Returns an InterfaceInfo object.
	/// </summary>
	public InterfaceInfo Interface { get; set; }
}
