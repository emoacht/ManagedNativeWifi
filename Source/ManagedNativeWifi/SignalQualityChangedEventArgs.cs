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

	internal SignalQualityChangedEventArgs(Guid interfaceId, int signalQuality)
	{
		this.InterfaceId = interfaceId;
		this.SignalQuality = signalQuality;
	}
}