﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagedNativeWifi;

/// <summary>
/// Wireless LAN information on available network
/// </summary>
public class AvailableNetworkInfo
{
	/// <summary>
	/// SSID (maximum 32 bytes)
	/// </summary>
	public NetworkIdentifier Ssid { get; }

	/// <summary>
	/// BSS network type
	/// </summary>
	public BssType BssType { get; }

	/// <summary>
	/// Signal quality (0-100)
	/// </summary>
	public int SignalQuality { get; }

	/// <summary>
	/// Whether security is enabled on this network
	/// </summary>
	public bool IsSecurityEnabled { get; }

	/// <summary>
	/// Associated wireless profile name
	/// </summary>
	public string ProfileName { get; }

	/// <summary>
	/// Default authentication algorithm to be used to connect to this network for the first time
	/// </summary>
	public AuthenticationAlgorithm AuthenticationAlgorithm { get; }

	/// <summary>
	/// Default cipher algorithm to be used to connect to this network
	/// </summary>
	public CipherAlgorithm CipherAlgorithm { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	public AvailableNetworkInfo(
		NetworkIdentifier ssid,
		BssType bssType,
		int signalQuality,
		bool isSecurityEnabled,
		string profileName,
		AuthenticationAlgorithm authenticationAlgorithm,
		CipherAlgorithm cipherAlgorithm)
	{
		this.Ssid = ssid;
		this.BssType = bssType;
		this.SignalQuality = signalQuality;
		this.IsSecurityEnabled = isSecurityEnabled;
		this.ProfileName = profileName;
		this.AuthenticationAlgorithm = authenticationAlgorithm;
		this.CipherAlgorithm = cipherAlgorithm;
	}
}

/// <summary>
/// Wireless LAN information on available network
/// </summary>
public class AvailableNetworkPack : AvailableNetworkInfo
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
	/// Constructor
	/// </summary>
	public AvailableNetworkPack(
		InterfaceInfo interfaceInfo,
		NetworkIdentifier ssid,
		BssType bssType,
		int signalQuality,
		bool isSecurityEnabled,
		string profileName,
		AuthenticationAlgorithm authenticationAlgorithm,
		CipherAlgorithm cipherAlgorithm) : base(
			ssid: ssid,
			bssType: bssType,
			signalQuality: signalQuality,
			isSecurityEnabled: isSecurityEnabled,
			profileName: profileName,
			authenticationAlgorithm: authenticationAlgorithm,
			cipherAlgorithm: cipherAlgorithm)
	{
		this.InterfaceInfo = interfaceInfo;
	}

	internal AvailableNetworkPack(
		InterfaceInfo interfaceInfo,
		AvailableNetworkInfo availableNetworkInfo) : this(
			interfaceInfo: interfaceInfo,
			ssid: availableNetworkInfo.Ssid,
			bssType: availableNetworkInfo.BssType,
			signalQuality: availableNetworkInfo.SignalQuality,
			isSecurityEnabled: availableNetworkInfo.IsSecurityEnabled,
			profileName: availableNetworkInfo.ProfileName,
			authenticationAlgorithm: availableNetworkInfo.AuthenticationAlgorithm,
			cipherAlgorithm: availableNetworkInfo.CipherAlgorithm)
	{ }
}

/// <summary>
/// Wireless LAN information on available network and group of associated BSS networks
/// </summary>
public class AvailableNetworkGroupPack : AvailableNetworkPack
{
	/// <summary>
	/// Associated BSS networks information
	/// </summary>
	public IReadOnlyCollection<BssNetworkPack> BssNetworks { get; }

	/// <summary>
	/// PHY type of associated BSS network which is the highest link quality
	/// </summary>
	public PhyType PhyType { get; }

	/// <summary>
	/// Link quality of associated BSS network which is the highest link quality
	/// </summary>
	public int LinkQuality { get; }

	/// <summary>
	/// Channel center frequency (KHz) of associated BSS network which has the highest link quality
	/// </summary>
	public int Frequency { get; }

	/// <summary>
	/// Frequency band (GHz) of associated BSS network which has the highest link quality
	/// </summary>
	public float Band { get; }

	/// <summary>
	/// Channel of associated BSS network which has the highest link quality
	/// </summary>
	public int Channel { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	public AvailableNetworkGroupPack(
		InterfaceInfo interfaceInfo,
		NetworkIdentifier ssid,
		BssType bssType,
		int signalQuality,
		bool isSecurityEnabled,
		string profileName,
		AuthenticationAlgorithm authenticationAlgorithm,
		CipherAlgorithm cipherAlgorithm,
		IEnumerable<BssNetworkPack> bssNetworks) : base(
			interfaceInfo: interfaceInfo,
			ssid: ssid,
			bssType: bssType,
			signalQuality: signalQuality,
			isSecurityEnabled: isSecurityEnabled,
			profileName: profileName,
			authenticationAlgorithm: authenticationAlgorithm,
			cipherAlgorithm: cipherAlgorithm)
	{
		this.BssNetworks = Array.AsReadOnly(bssNetworks?.OrderByDescending(x => x.LinkQuality).ToArray() ?? []);
		if (this.BssNetworks is not { Count: > 0 })
			return;

		var highestLinkQualityNetwork = this.BssNetworks.First();

		PhyType = highestLinkQualityNetwork.PhyType;
		LinkQuality = highestLinkQualityNetwork.LinkQuality;
		Frequency = highestLinkQualityNetwork.Frequency;
		Band = highestLinkQualityNetwork.Band;
		Channel = highestLinkQualityNetwork.Channel;
	}
}