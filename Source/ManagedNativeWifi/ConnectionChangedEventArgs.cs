using System;

namespace ManagedNativeWifi;

/// <summary>
/// Connection state enumeration values.
/// </summary>
public enum ConnectionState
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
public class ConnectionChangedEventArgs : EventArgs
{
	private readonly InterfaceInfo _interfaceInfo;
	private readonly ConnectionState _connectionState;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ConnectionChangedEventArgs" /> class.
	/// </summary>
	/// <param name="interfaceInfo">An instance of the <see cref="T:InterfaceInfo" /> class</param>
	/// <param name="connectionState">One of the <see cref="T:ConnectionState" /> values.</param>
	public ConnectionChangedEventArgs(InterfaceInfo interfaceInfo, ConnectionState connectionState)
	{
		_interfaceInfo = interfaceInfo;
		_connectionState = connectionState;
	}

	/// <summary>
	/// Returns a value from the <see cref="T:ConnectionState" /> enumerator.
	/// </summary>
	public ConnectionState State => _connectionState;

	/// <summary>
	/// Returns an <see cref="T:InterfaceInfo" /> object.
	/// </summary>
	public InterfaceInfo Interface => _interfaceInfo;
}
