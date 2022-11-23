using System;

namespace ManagedNativeWifi;

/// <summary>
/// Provides data for the ConnectionChanged event.
/// </summary>
public class ConnectionChangedEventArgs : EventArgs
{
	/// <summary>
	/// Associated wireless interface ID
	/// </summary>
	public Guid InterfaceId { get; }

	/// <summary>
	/// Connection changed state
	/// </summary>
	public ConnectionChangedState ChangedState { get; }

	/// <summary>
	/// Connection notification data
	/// </summary>
	public ConnectionNotificationData Data { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ConnectionChangedEventArgs"/> class.
	/// </summary>
	/// <param name="interfaceId">Interface ID</param>
	/// <param name="changedState">Connection changed state</param>
	/// <param name="data">Connection notification data</param>
	public ConnectionChangedEventArgs(Guid interfaceId, ConnectionChangedState changedState, ConnectionNotificationData data)
	{
		this.InterfaceId = interfaceId;
		this.ChangedState = changedState;
		this.Data = data;
	}
}

/// <summary>
/// Connection changed state
/// </summary>
public enum ConnectionChangedState
{
	/// <summary>
	/// Unknown (invalid value)
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// A connection attempt has started.
	/// </summary>
	Started,

	/// <summary>
	/// A connection has completed.
	/// </summary>
	Completed,

	/// <summary>
	/// A connection attempt has failed.
	/// </summary>
	Failed,

	/// <summary>
	/// The current connection is disconnecting.
	/// </summary>
	Disconnecting,

	/// <summary>
	/// The current connection has disconnected.
	/// </summary>
	Disconnected
}