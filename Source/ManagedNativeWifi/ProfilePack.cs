﻿using System;

namespace ManagedNativeWifi;

/// <summary>
/// Wireless profile information
/// </summary>
public class ProfilePack
{
	/// <summary>
	/// Associated wireless interface information
	/// </summary>
	public InterfaceInfo InterfaceInfo { get; }

	/// <summary>
	/// Associated wireless interface information
	/// </summary>
	[Obsolete("Use InterfaceInfo property instead.")]
	public InterfaceInfo Interface => InterfaceInfo;

	/// <summary>
	/// Profile name
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Profile type
	/// </summary>
	public ProfileType ProfileType { get; }

	/// <summary>
	/// Profile XML document
	/// </summary>
	public ProfileDocument Document { get; }

	/// <summary>
	/// Profile XML string
	/// </summary>
	[Obsolete("Use Document.Xml method instead.")]
	public string ProfileXml => Document.Xml;

	/// <summary>
	/// SSID of associated wireless LAN
	/// </summary>
	[Obsolete("Use Document.Ssid property instead.")]
	public NetworkIdentifier Ssid => Document.Ssid;

	/// <summary>
	/// BSS network type of associated wireless LAN
	/// </summary>
	[Obsolete("Use Document.BssType property instead.")]
	public BssType BssType => Document.BssType;

	/// <summary>
	/// Authentication of associated wireless LAN
	/// </summary>
	[Obsolete("Use Document.Authentication property instead.")]
	public string Authentication => Document.AuthenticationString;

	/// <summary>
	/// Encryption of associated wireless LAN
	/// </summary>
	[Obsolete("Use Document.Encryption property instead.")]
	public string Encryption => Document.EncryptionString;

	/// <summary>
	/// Whether this profile is set to be automatically connected
	/// </summary>
	[Obsolete("Use Document.IsAutoConnectEnabled property instead.")]
	public bool IsAutomatic => Document.IsAutoConnectEnabled;

	/// <summary>
	/// Position in preference order of associated wireless interface
	/// </summary>
	public int Position { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	public ProfilePack(
		InterfaceInfo interfaceInfo,
		string name,
		ProfileType profileType,
		string profileXml,
		int position)
	{
		this.InterfaceInfo = interfaceInfo;
		this.Name = name;
		this.ProfileType = profileType;
		Document = new ProfileDocument(profileXml);
		this.Position = position;
	}
}

/// <summary>
/// Wireless profile and related radio information
/// </summary>
public class ProfileRadioPack : ProfilePack
{
	/// <summary>
	/// Associated wireless interface and associated information
	/// </summary>
	public new InterfaceConnectionInfo InterfaceInfo => (InterfaceConnectionInfo)base.InterfaceInfo;

	/// <summary>
	/// Associated wireless interface and associated information
	/// </summary>
	[Obsolete("Use InterfaceInfo property instead.")]
	public new InterfaceConnectionInfo Interface => InterfaceInfo;

	/// <summary>
	/// Whether radio of associated wireless interface is on
	/// </summary>
	public bool IsRadioOn => InterfaceInfo.IsRadioOn;

	/// <summary>
	/// Whether associated wireless interface is connected to associated wireless LAN
	/// </summary>
	public bool IsConnected { get; }

	/// <summary>
	/// PHY type of associated wireless LAN
	/// </summary>
	public PhyType PhyType { get; }

	/// <summary>
	/// Signal quality of associated wireless LAN
	/// </summary>
	public int SignalQuality { get; }

	/// <summary>
	/// Link quality of associated wireless LAN
	/// </summary>
	public int LinkQuality { get; }

	/// <summary>
	/// Channel center frequency (KHz) of associated wireless LAN
	/// </summary>
	public int Frequency { get; }

	/// <summary>
	/// Frequency band (GHz) of associated wireless LAN
	/// </summary>
	public float Band { get; }

	/// <summary>
	/// Channel of associated wireless LAN
	/// </summary>
	public int Channel { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	public ProfileRadioPack(
		InterfaceConnectionInfo interfaceInfo,
		string name,
		bool isConnected,
		ProfileType profileType,
		string profileXml,
		int position,
		PhyType phyType,
		int signalQuality,
		int linkQuality,
		int frequency,
		float band,
		int channel) : base(
			interfaceInfo: interfaceInfo,
			name: name,
			profileType: profileType,
			profileXml: profileXml,
			position: position)
	{
		this.IsConnected = isConnected;
		this.PhyType = phyType;
		this.SignalQuality = signalQuality;
		this.LinkQuality = linkQuality;
		this.Frequency = frequency;
		this.Band = band;
		this.Channel = channel;
	}
}