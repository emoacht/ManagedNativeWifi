using System;

namespace ManagedNativeWifi;

/// <summary>
/// Provides data for the SignalQualityChanged event.
/// </summary>
public class SignalQualityChangedEventArgs : EventArgs
{
	/// <summary>
	/// Associated wireless interface ID
	/// </summary>
	public Guid InterfaceId { get; }

	/// <summary>
	/// New signal quality (0-100)
	/// </summary>
	public int SignalQuality { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="SignalQualityChangedEventArgs"/> class.
	/// </summary>
	/// <param name="interfaceId">Interface ID</param>
	/// <param name="signalQuality">New signal quality</param>
	public SignalQualityChangedEventArgs(Guid interfaceId, int signalQuality)
	{
		InterfaceId = interfaceId;
		SignalQuality = signalQuality;
	}
}