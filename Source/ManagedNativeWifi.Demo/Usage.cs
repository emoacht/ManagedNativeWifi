using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedNativeWifi.Demo;

public static class Usage
{
	/// <summary>
	/// Enumerates SSIDs of available wireless LANs.
	/// </summary>
	/// <returns>SSID strings</returns>
	public static IEnumerable<string> EnumerateNetworkSsids()
	{
		return NativeWifi.EnumerateAvailableNetworkSsids()
			.Select(x => x.ToString()); // UTF-8 string representation
	}

	/// <summary>
	/// Enumerates tuples of SSID and signal quality of available wireless LANs.
	/// </summary>
	/// <returns>Tuples of SSID string and signal quality</returns>
	public static IEnumerable<(string ssidString, int signalQuality)>
		EnumerateNetworkSsidsAndSignalQualities()
	{
		return NativeWifi.EnumerateAvailableNetworks()
			.Select(x => (x.Ssid.ToString(), x.SignalQuality));
	}

	/// <summary>
	/// Connects to the available wireless LAN which has the highest signal quality.
	/// </summary>
	/// <returns>True if successfully connected. False if not.</returns>
	public static async Task<bool> ConnectAsync()
	{
		var availableNetwork = NativeWifi.EnumerateAvailableNetworks()
			.Where(x => !string.IsNullOrWhiteSpace(x.ProfileName))
			.OrderByDescending(x => x.SignalQuality)
			.FirstOrDefault();

		if (availableNetwork is null)
			return false;

		return await NativeWifi.ConnectNetworkAsync(
			interfaceId: availableNetwork.InterfaceInfo.Id,
			profileName: availableNetwork.ProfileName,
			bssType: availableNetwork.BssType,
			timeout: TimeSpan.FromSeconds(10));
	}

	/// <summary>
	/// Refreshes available wireless LANs.
	/// </summary>
	public static Task RefreshAsync()
	{
		return NativeWifi.ScanNetworksAsync(timeout: TimeSpan.FromSeconds(10));
	}

	/// <summary>
	/// Refreshes available wireless LANs using wireless interfaces that are not connected.
	/// </summary>
	public static Task RefreshNotConnectedAsync()
	{
		return NativeWifi.ScanNetworksAsync(
			mode: ScanMode.OnlyNotConnected,
			null,
			null,
			timeout: TimeSpan.FromSeconds(10),
			CancellationToken.None);
	}

	/// <summary>
	/// Refreshes available wireless LANs using wireless interfaces that are specified.
	/// </summary>
	public static Task RefreshSpecifiedAsync(Guid interfaceId)
	{
		return NativeWifi.ScanNetworksAsync(
			mode: ScanMode.OnlySpecified,
			interfaceIds: [interfaceId],
			null,
			timeout: TimeSpan.FromSeconds(10),
			CancellationToken.None);
	}

	/// <summary>
	/// Refreshes available wireless LANs with a specified SSID string.
	/// </summary>
	public static Task RefreshSsidAsync(string ssidString)
	{
		return NativeWifi.ScanNetworksAsync(
			mode: ScanMode.All,
			null,
			ssid: new NetworkIdentifier(ssidString),
			timeout: TimeSpan.FromSeconds(10),
			CancellationToken.None);
	}

	/// <summary>
	/// Deletes a specified wireless profile.
	/// </summary>
	/// <param name="profileName">Profile name to be deleted</param>
	/// <returns>True if successfully deleted. False if not.</returns>
	/// <remarks>Profile name is case-sensitive.</remarks>
	public static bool DeleteProfile(string profileName)
	{
		var targetProfile = NativeWifi.EnumerateProfiles()
			.Where(x => profileName.Equals(x.Name, StringComparison.Ordinal))
			.FirstOrDefault();

		if (targetProfile is null)
			return false;

		return NativeWifi.DeleteProfile(
			interfaceId: targetProfile.InterfaceInfo.Id,
			profileName: profileName);
	}

	/// <summary>
	/// Enumerates wireless LAN channels whose RSSI go beyond a specified threshold.
	/// </summary>
	/// <param name="rssiThreshold">Threshold of RSSI</param>
	/// <returns>Channel numbers</returns>
	public static IEnumerable<int> EnumerateNetworkChannels(int rssiThreshold)
	{
		return NativeWifi.EnumerateBssNetworks()
			.Where(x => x.Rssi > rssiThreshold)
			.Select(x => x.Channel);
	}

	/// <summary>
	/// Shows wireless connection information of connected wireless interfaces.
	/// </summary>
	public static void ShowConnectedNetworkInformation()
	{
		foreach (var interfaceId in NativeWifi.EnumerateInterfaces()
			.Where(x => x.State is InterfaceState.Connected)
			.Select(x => x.Id))
		{
			// Following methods work only with connected wireless interfaces.
			var (result, cc) = NativeWifi.GetCurrentConnection(interfaceId);
			if (result is ActionResult.Success)
			{
				Trace.WriteLine($"Profile: {cc.ProfileName}");
				Trace.WriteLine($"SSID: {cc.Ssid}");
				Trace.WriteLine($"PHY type: 802.11{cc.PhyType.ToProtocolName()}");
				Trace.WriteLine($"Authentication algorithm: {cc.AuthenticationAlgorithm}");
				Trace.WriteLine($"Cipher algorithm: {cc.CipherAlgorithm}");
				Trace.WriteLine($"Signal quality: {cc.SignalQuality}");
				Trace.WriteLine($"Rx rate: {cc.RxRate} Kbps");
				Trace.WriteLine($"Tx rate: {cc.TxRate} Kbps");
			}

			// GetRealtimeConnectionQuality method works only on Windows 11 24H2.
			(result, var rcq) = NativeWifi.GetRealtimeConnectionQuality(interfaceId);
			if (result is ActionResult.Success)
			{
				Trace.WriteLine($"PHY type: 802.11{rcq.PhyType.ToProtocolName()}");
				Trace.WriteLine($"Link quality: {rcq.LinkQuality}");
				Trace.WriteLine($"Rx rate: {rcq.RxRate} Kbps");
				Trace.WriteLine($"Tx rate: {rcq.TxRate} Kbps");
				Trace.WriteLine($"MLO connection: {rcq.IsMultiLinkOperation}");

				if (rcq.Links.Count > 0)
				{
					var link = rcq.Links[0];
					Trace.WriteLine($"RSSI: {link.Rssi}");
					Trace.WriteLine($"Frequency: {link.Frequency} MHz");
					Trace.WriteLine($"Bandwidth: {link.Bandwidth} MHz");
				}
			}
			else if (result is ActionResult.NotSupported)
			{
				(result, int rssi) = NativeWifi.GetRssi(interfaceId);
				if (result is ActionResult.Success)
				{
					Trace.WriteLine($"RSSI: {rssi}");
				}
			}
		}
	}

	/// <summary>
	/// Turns on the radio of a wireless interface which is not currently on but can be on.
	/// </summary>
	/// <returns>True if successfully turned on. False if not.</returns>
	public static async Task<bool> TurnOnAsync()
	{
		var targetInterface = NativeWifi.EnumerateInterfaces()
			.FirstOrDefault(x =>
			{
				var radioState = NativeWifi.GetRadio(x.Id)?.RadioStates.FirstOrDefault();
				if (radioState is null)
					return false;

				if (!radioState.IsHardwareOn) // Hardware radio state is off.
					return false;

				return !radioState.IsSoftwareOn; // Software radio state is off.
			});

		if (targetInterface is null)
			return false;

		try
		{
			return await Task.Run(() => NativeWifi.TurnOnRadio(targetInterface.Id));
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
	}

	/// <summary>
	/// Changes the automatic connection and automatic switch elements of a wireless profile.
	/// </summary>
	/// <param name="enableAutoConnect">Whether automatic connection should be enabled</param>
	/// <param name="enableAutoSwitch">Whether automatic switch should be enabled</param>
	/// <returns>True if successfully changed. False if not.</returns>
	/// <remarks>A wireless profile made by group policy will be skipped.</remarks>
	public static bool ChangeProfile(bool enableAutoConnect, bool enableAutoSwitch)
	{
		var targetProfile = NativeWifi.EnumerateProfiles()
			.FirstOrDefault(x => x.ProfileType is not ProfileType.GroupPolicy);

		if (targetProfile is null)
			return false;

		if ((targetProfile.Document.IsAutoConnectEnabled == enableAutoConnect) &&
			(targetProfile.Document.IsAutoSwitchEnabled == enableAutoSwitch))
			return false;

		// Set IsAutoConnectEnabled first.
		targetProfile.Document.IsAutoConnectEnabled = enableAutoConnect;
		targetProfile.Document.IsAutoSwitchEnabled = enableAutoSwitch;

		return NativeWifi.SetProfile(
			interfaceId: targetProfile.InterfaceInfo.Id,
			profileType: targetProfile.ProfileType,
			profileXml: targetProfile.Document.Xml,
			profileSecurity: null, // No change
			overwrite: true);
	}
}