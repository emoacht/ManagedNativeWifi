using System;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi;

/// <summary>
/// Wireless interface information
/// </summary>
public class InterfaceInfo
{
	/// <summary>
	/// Interface ID
	/// </summary>
	public Guid Id { get; }

	/// <summary>
	/// Interface description
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// Interface state
	/// </summary>
	public InterfaceState State { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	public InterfaceInfo(Guid id, string description, InterfaceState state)
	{
		this.Id = id;
		this.Description = description;
		this.State = state;
	}

	internal InterfaceInfo(WLAN_INTERFACE_INFO info)
	{
		Id = info.InterfaceGuid;
		Description = info.strInterfaceDescription;
		State = InterfaceStateConverter.Convert(info.isState);
	}
}

/// <summary>
/// Wireless interface and related connection information
/// </summary>
public class InterfaceConnectionInfo : InterfaceInfo
{
	/// <summary>
	/// Whether the radio of the wireless interface is on
	/// </summary>
	public bool IsRadioOn { get; }

	/// <summary>
	/// Whether the wireless interface is connected to a wireless LAN
	/// </summary>
	public bool IsConnected { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	internal InterfaceConnectionInfo(
		WLAN_INTERFACE_INFO info,
		bool isRadioOn,
		bool isConnected) : base(info)
	{
		this.IsRadioOn = isRadioOn;
		this.IsConnected = isConnected;
	}
}