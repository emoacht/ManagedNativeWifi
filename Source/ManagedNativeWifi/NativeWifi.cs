using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using ManagedNativeWifi.Common;
using static ManagedNativeWifi.Win32.NativeMethod;
using Base = ManagedNativeWifi.Win32.BaseMethod;

namespace ManagedNativeWifi;

/// <summary>
/// A managed implementation of Native Wifi API
/// </summary>
public class NativeWifi
{
	#region Enumerate interfaces

	/// <summary>
	/// Enumerates wireless interface information.
	/// </summary>
	/// <returns>Wireless interface information</returns>
	public static IEnumerable<InterfaceInfo> EnumerateInterfaces()
	{
		return EnumerateInterfaces(null);
	}

	internal static IEnumerable<InterfaceInfo> EnumerateInterfaces(Base.WlanClient client)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		return Base.GetInterfaceInfoList(container.Content.Handle)
			.Select(x => new InterfaceInfo(x));
	}

	/// <summary>
	/// Enumerates wireless interface and related connection information.
	/// </summary>
	/// <returns>Wireless interface and related connection information</returns>
	public static IEnumerable<InterfaceConnectionInfo> EnumerateInterfaceConnections()
	{
		return EnumerateInterfaceConnections(null);
	}

	internal static IEnumerable<InterfaceConnectionInfo> EnumerateInterfaceConnections(Base.WlanClient client)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		foreach (var interfaceInfo in Base.GetInterfaceInfoList(container.Content.Handle))
		{
			var isConnected = (interfaceInfo.isState is WLAN_INTERFACE_STATE.wlan_interface_state_connected);
			var isRadioOn = isConnected ||
				EnumerateRadioStates(container.Content, interfaceInfo.InterfaceGuid).Any(x => x.IsOn);

			yield return new InterfaceConnectionInfo(
				interfaceInfo,
				isRadioOn: isRadioOn,
				isConnected: isConnected);
		}
	}

	#endregion

	#region Scan networks

	/// <summary>
	/// Asynchronously requests wireless interfaces to scan wireless LANs.
	/// </summary>
	/// <param name="timeout">Timeout duration</param>
	/// <returns>Wireless interface IDs that were successfully scanned before the timeout</returns>
	public static Task<IEnumerable<Guid>> ScanNetworksAsync(TimeSpan timeout)
	{
		return ScanNetworksAsync(null, ScanMode.All, interfaceIds: null, ssid: null, timeout, CancellationToken.None);
	}

	/// <summary>
	/// Asynchronously requests wireless interfaces to scan wireless LANs.
	/// </summary>
	/// <param name="timeout">Timeout duration</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Wireless interface IDs that were successfully scanned before the timeout</returns>
	public static Task<IEnumerable<Guid>> ScanNetworksAsync(TimeSpan timeout, CancellationToken cancellationToken)
	{
		return ScanNetworksAsync(null, ScanMode.All, interfaceIds: null, ssid: null, timeout, cancellationToken);
	}

	/// <summary>
	/// Asynchronously requests wireless interfaces to scan wireless LANs.
	/// </summary>
	/// <param name="mode">Mode to scan</param>
	/// <param name="interfaceIds">Wireless interface IDs to specify wireless interfaces when mode is OnlySpecified</param>
	/// <param name="timeout">Timeout duration</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Wireless interface IDs that were successfully scanned before the timeout</returns>
	public static Task<IEnumerable<Guid>> ScanNetworksAsync(ScanMode mode, IEnumerable<Guid> interfaceIds, TimeSpan timeout, CancellationToken cancellationToken)
	{
		return ScanNetworksAsync(null, mode, interfaceIds, ssid: null, timeout, cancellationToken);
	}

	/// <summary>
	/// Asynchronously requests wireless interfaces to scan wireless LANs.
	/// </summary>
	/// <param name="mode">Mode to scan</param>
	/// <param name="interfaceIds">Wireless interface IDs to specify wireless interfaces when mode is OnlySpecified</param>
	/// <param name="ssid">SSID of wireless LAN to be scanned</param>
	/// <param name="timeout">Timeout duration</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Wireless interface IDs that were successfully scanned before the timeout</returns>
	public static Task<IEnumerable<Guid>> ScanNetworksAsync(ScanMode mode, IEnumerable<Guid> interfaceIds, NetworkIdentifier ssid, TimeSpan timeout, CancellationToken cancellationToken)
	{
		return ScanNetworksAsync(null, mode, interfaceIds, ssid, timeout, cancellationToken);
	}

	internal static async Task<IEnumerable<Guid>> ScanNetworksAsync(Base.WlanNotificationClient client, ScanMode mode, IEnumerable<Guid> interfaceIds, NetworkIdentifier ssid, TimeSpan timeout, CancellationToken cancellationToken)
	{
		if (timeout <= TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(timeout), "The timeout duration must be positive.");

		var specifiedIds = interfaceIds?.Where(x => x != Guid.Empty).ToArray();

		if ((mode is ScanMode.OnlySpecified) && (specifiedIds is not { Length: > 0 }))
			throw new ArgumentException("The interface IDs must be provided when mode is OnlySpecified.", nameof(interfaceIds));

		var dot11Ssid = default(DOT11_SSID);
		if ((ssid is not null) && !DOT11_SSID.TryCreate(ssid.ToBytes(), out dot11Ssid))
			throw new ArgumentException("The specified SSID is invalid", nameof(ssid));

		using var container = new DisposableContainer<Base.WlanNotificationClient>(client);

		var targetIds = mode switch
		{
			ScanMode.All => Base.GetInterfaceInfoList(container.Content.Handle)
				.Select(x => x.InterfaceGuid)
				.ToArray(),
			ScanMode.OnlyNotConnected => Base.GetInterfaceInfoList(container.Content.Handle)
				.Where(x => (x.isState is WLAN_INTERFACE_STATE.wlan_interface_state_disconnected))
				.Select(x => x.InterfaceGuid)
				.ToArray(),
			ScanMode.OnlySpecified => specifiedIds,
			_ => []
		};

		var tcs = new TaskCompletionSource<bool>();
		var counter = new ScanCounter(() => Task.Run(() => tcs.TrySetResult(true)), targetIds);

		container.Content.Register(WLAN_NOTIFICATION_SOURCE.WLAN_NOTIFICATION_SOURCE_ACM);
		container.Content.NotificationReceived += (_, data) =>
		{
			switch ((WLAN_NOTIFICATION_ACM)data.NotificationCode)
			{
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_scan_complete:
					counter.SetSuccess(data.InterfaceGuid);
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_scan_fail:
					counter.SetFailure(data.InterfaceGuid);
					break;
			}
		};

		foreach (var targetId in targetIds)
		{
			var result = Base.Scan(container.Content.Handle, targetId, dot11Ssid);
			if (!result)
				counter.SetFailure(targetId);
		}

		using (cancellationToken.Register(() => tcs.TrySetCanceled()))
		{
			var scanTask = tcs.Task;
			await Task.WhenAny(scanTask, Task.Delay(timeout, cancellationToken));

			return counter.Results;
		}
	}

	private class ScanCounter
	{
		private readonly Action _complete;
		private readonly List<Guid> _targets = [];
		private readonly List<Guid> _results = [];

		public IEnumerable<Guid> Results => _results.ToArray();

		public ScanCounter(Action complete, IEnumerable<Guid> targets)
		{
			this._complete = complete;
			this._targets.AddRange(targets);
		}

		private readonly object _lock = new();

		public void SetSuccess(Guid value)
		{
			lock (_lock)
			{
				_targets.Remove(value);
				_results.Add(value);

				CheckTargets();
			}
		}

		public void SetFailure(Guid value)
		{
			lock (_lock)
			{
				_targets.Remove(value);

				CheckTargets();
			}
		}

		private void CheckTargets()
		{
			if (_targets.Count > 0)
				return;

			_complete.Invoke();
		}
	}

	#endregion

	#region Enumerate networks

	/// <summary>
	/// Enumerates SSIDs of available wireless LANs.
	/// </summary>
	/// <returns>SSIDs</returns>
	public static IEnumerable<NetworkIdentifier> EnumerateAvailableNetworkSsids()
	{
		return EnumerateAvailableNetworkSsids(null);
	}

	internal static IEnumerable<NetworkIdentifier> EnumerateAvailableNetworkSsids(Base.WlanClient client)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		foreach (var interfaceInfo in Base.GetInterfaceInfoList(container.Content.Handle))
		{
			foreach (var availableNetwork in Base.GetAvailableNetworkList(container.Content.Handle, interfaceInfo.InterfaceGuid).list)
				yield return new NetworkIdentifier(availableNetwork.dot11Ssid);
		}
	}

	/// <summary>
	/// Enumerates SSIDs of connected wireless LANs.
	/// </summary>
	/// <returns>SSIDs</returns>
	public static IEnumerable<NetworkIdentifier> EnumerateConnectedNetworkSsids()
	{
		return EnumerateConnectedNetworkSsids(null);
	}

	internal static IEnumerable<NetworkIdentifier> EnumerateConnectedNetworkSsids(Base.WlanClient client)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		foreach (var interfaceInfo in Base.GetInterfaceInfoList(container.Content.Handle))
		{
			if (interfaceInfo.isState is not WLAN_INTERFACE_STATE.wlan_interface_state_connected)
				continue;

			var (result, value) = Base.GetCurrentConnection(container.Content.Handle, interfaceInfo.InterfaceGuid);
			if (result is not ActionResult.Success)
				continue;

			var association = value.wlanAssociationAttributes;

			yield return new NetworkIdentifier(association.dot11Ssid);
		}
	}

	/// <summary>
	/// Enumerates wireless LAN information on available networks.
	/// </summary>
	/// <returns>Wireless LAN information on available networks</returns>
	/// <remarks>
	/// If multiple profiles are associated with a same network, there will be multiple entries
	/// with the same SSID.
	/// </remarks>
	public static IEnumerable<AvailableNetworkPack> EnumerateAvailableNetworks()
	{
		return EnumerateAvailableNetworks((Base.WlanClient)null);
	}

	internal static IEnumerable<AvailableNetworkPack> EnumerateAvailableNetworks(Base.WlanClient client)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		foreach (var interfaceInfo in EnumerateInterfaces(container.Content))
		{
			var (_, list) = Base.GetAvailableNetworkList(container.Content.Handle, interfaceInfo.Id);

			foreach (var availableNetworkInfo in EnumerateAvailableNetworks(list))
				yield return new AvailableNetworkPack(interfaceInfo, availableNetworkInfo);
		}
	}

	/// <summary>
	/// Enumerates wireless LAN information on available networks associated with a specified
	/// wireless interface.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <returns>
	/// <para>
	/// result: Action result.
	/// Success if the interface is found and the function succeeded.
	/// Other if the interface is is found or the function failed.
	/// </para>
	/// <para>list: Wireless LAN information on available networks if succeeded</para>
	/// </returns>
	public static (ActionResult result, IEnumerable<AvailableNetworkInfo> list) EnumerateAvailableNetworks(Guid interfaceId)
	{
		return EnumerateAvailableNetworks(null, interfaceId);
	}

	internal static (ActionResult result, IEnumerable<AvailableNetworkInfo> list) EnumerateAvailableNetworks(Base.WlanClient client, Guid interfaceId)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		var (result, list) = Base.GetAvailableNetworkList(container.Content.Handle, interfaceId);
		if (result is not ActionResult.Success)
			return (result, null);

		return (ActionResult.Success, EnumerateAvailableNetworks(list));
	}

	private static IEnumerable<AvailableNetworkInfo> EnumerateAvailableNetworks(IEnumerable<WLAN_AVAILABLE_NETWORK> availableNetworks)
	{
		foreach (var availableNetwork in availableNetworks)
		{
			if (!BssTypeConverter.TryConvert(availableNetwork.dot11BssType, out BssType bssType) ||
				!AuthenticationAlgorithmConverter.TryConvert(availableNetwork.dot11DefaultAuthAlgorithm, out AuthenticationAlgorithm authenticationAlgorithm) ||
				!CipherAlgorithmConverter.TryConvert(availableNetwork.dot11DefaultCipherAlgorithm, out CipherAlgorithm cipherAlgorithm))
				continue;

			yield return new AvailableNetworkInfo(
				ssid: new NetworkIdentifier(availableNetwork.dot11Ssid),
				bssType: bssType,
				isConnectable: availableNetwork.bNetworkConnectable,
				signalQuality: (int)availableNetwork.wlanSignalQuality,
				isSecurityEnabled: availableNetwork.bSecurityEnabled,
				profileName: availableNetwork.strProfileName,
				authenticationAlgorithm: authenticationAlgorithm,
				cipherAlgorithm: cipherAlgorithm);
		}
	}

	/// <summary>
	/// Enumerates wireless LAN information on available networks and group of associated BSS
	/// networks.
	/// </summary>
	/// <returns>Wireless LAN information on available networks and group of associated BSS networks</returns>
	/// <remarks>
	/// If multiple profiles are associated with a same network, there will be multiple entries with
	/// the same SSID.
	/// </remarks>
	public static IEnumerable<AvailableNetworkGroupPack> EnumerateAvailableNetworkGroups()
	{
		return EnumerateAvailableNetworkGroups(null);
	}

	internal static IEnumerable<AvailableNetworkGroupPack> EnumerateAvailableNetworkGroups(Base.WlanClient client)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		foreach (var interfaceInfo in EnumerateInterfaces(container.Content))
		{
			foreach (var availableNetworkGroup in EnumerateAvailableNetworkGroups(container.Content, interfaceInfo))
				yield return availableNetworkGroup;
		}
	}

	private static IEnumerable<AvailableNetworkGroupPack> EnumerateAvailableNetworkGroups(Base.WlanClient client, InterfaceInfo interfaceInfo)
	{
		foreach (var availableNetwork in Base.GetAvailableNetworkList(client.Handle, interfaceInfo.Id).list)
		{
			if (!BssTypeConverter.TryConvert(availableNetwork.dot11BssType, out BssType bssType) ||
				!AuthenticationAlgorithmConverter.TryConvert(availableNetwork.dot11DefaultAuthAlgorithm, out AuthenticationAlgorithm authenticationAlgorithm) ||
				!CipherAlgorithmConverter.TryConvert(availableNetwork.dot11DefaultCipherAlgorithm, out CipherAlgorithm cipherAlgorithm))
				continue;

			var bssNetworks = Base.GetNetworkBssEntryList(client.Handle, interfaceInfo.Id,
				availableNetwork.dot11Ssid, availableNetwork.dot11BssType, availableNetwork.bSecurityEnabled).list
				.Select(x => TryConvertBssNetwork(x, out BssNetworkInfo bssNetworkInfo) ? new BssNetworkPack(interfaceInfo, bssNetworkInfo) : null)
				.Where(x => x is not null);

			yield return new AvailableNetworkGroupPack(
				interfaceInfo: interfaceInfo,
				ssid: new NetworkIdentifier(availableNetwork.dot11Ssid),
				bssType: bssType,
				isConnectable: availableNetwork.bNetworkConnectable,
				signalQuality: (int)availableNetwork.wlanSignalQuality,
				isSecurityEnabled: availableNetwork.bSecurityEnabled,
				profileName: availableNetwork.strProfileName,
				authenticationAlgorithm: authenticationAlgorithm,
				cipherAlgorithm: cipherAlgorithm,
				bssNetworks: bssNetworks);
		}
	}

	/// <summary>
	/// Enumerates wireless LAN information on BSS networks.
	/// </summary>
	/// <returns>Wireless LAN information on BSS networks</returns>
	public static IEnumerable<BssNetworkPack> EnumerateBssNetworks()
	{
		return EnumerateBssNetworks((Base.WlanClient)null);
	}

	internal static IEnumerable<BssNetworkPack> EnumerateBssNetworks(Base.WlanClient client)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		foreach (var interfaceInfo in EnumerateInterfaces(container.Content))
		{
			var (_, list) = Base.GetNetworkBssEntryList(container.Content.Handle, interfaceInfo.Id);

			foreach (var bssNetworkInfo in EnumerateBssNetworks(list))
				yield return new BssNetworkPack(interfaceInfo, bssNetworkInfo);
		}
	}

	/// <summary>
	/// Enumerates wireless LAN information on BSS networks associated with a specified
	/// wireless interface. 
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <returns>
	/// <para>
	/// result: Action result.
	/// Success if the interface is found and the function succeeded.
	/// Other if the interface is is found or the function failed.
	/// </para>
	/// <para>list: Wireless LAN information on BSS networks if succeeded</para>
	/// </returns>
	public static (ActionResult result, IEnumerable<BssNetworkInfo> list) EnumerateBssNetworks(Guid interfaceId)
	{
		return EnumerateBssNetworks(null, interfaceId);
	}

	internal static (ActionResult result, IEnumerable<BssNetworkInfo> list) EnumerateBssNetworks(Base.WlanClient client, Guid interfaceId)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		var (result, list) = Base.GetNetworkBssEntryList(container.Content.Handle, interfaceId);
		if (result is not ActionResult.Success)
			return (result, null);

		return (ActionResult.Success, EnumerateBssNetworks(list));
	}

	private static IEnumerable<BssNetworkInfo> EnumerateBssNetworks(WLAN_BSS_ENTRY[] bssEntries)
	{
		foreach (var bssEntry in bssEntries)
		{
			if (TryConvertBssNetwork(bssEntry, out BssNetworkInfo bssNetworkInfo))
				yield return bssNetworkInfo;
		}
	}

	private static bool TryConvertBssNetwork(WLAN_BSS_ENTRY bssEntry, out BssNetworkInfo bssNetworkInfo)
	{
		if (BssTypeConverter.TryConvert(bssEntry.dot11BssType, out BssType bssType))
		{
			TryDetectBandChannel(bssEntry.ulChCenterFrequency, out float band, out int channel);

			bssNetworkInfo = new BssNetworkInfo(
				ssid: new NetworkIdentifier(bssEntry.dot11Ssid),
				bssType: bssType,
				bssid: new NetworkIdentifier(bssEntry.dot11Bssid),
				phyType: PhyTypeConverter.Convert(bssEntry.dot11BssPhyType),
				rssi: bssEntry.lRssi,
				linkQuality: (int)bssEntry.uLinkQuality,
				frequency: (int)bssEntry.ulChCenterFrequency,
				band: band,
				channel: channel);
			return true;
		}
		bssNetworkInfo = null;
		return false;
	}

	#endregion

	#region Get connection/connection quality

	/// <summary>
	/// Gets wireless connection information associated with a specified wireless interface.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <returns>
	/// <para>
	/// result: Action result.
	/// Success if the interface is connected and the function succeeded.
	/// Other if the interface is not connected or the function failed.
	/// </para>
	/// <para>value: Wireless connection information if succeeded</para>
	/// </returns>
	public static (ActionResult result, CurrentConnectionInfo value) GetCurrentConnection(Guid interfaceId)
	{
		return GetCurrentConnection(null, interfaceId);
	}

	internal static (ActionResult result, CurrentConnectionInfo value) GetCurrentConnection(Base.WlanClient client, Guid interfaceId)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		var (result, value) = Base.GetCurrentConnection(container.Content.Handle, interfaceId);
		if (result is not ActionResult.Success)
			return (result, null);

		if (!BssTypeConverter.TryConvert(value.wlanAssociationAttributes.dot11BssType, out BssType bssType) ||
			!AuthenticationAlgorithmConverter.TryConvert(value.wlanSecurityAttributes.dot11AuthAlgorithm, out AuthenticationAlgorithm authenticationAlgorithm) ||
			!CipherAlgorithmConverter.TryConvert(value.wlanSecurityAttributes.dot11CipherAlgorithm, out CipherAlgorithm cipherAlgorithm))
			return (ActionResult.OtherError, null);

		return (ActionResult.Success, new CurrentConnectionInfo(
			interfaceState: InterfaceStateConverter.Convert(value.isState),
			connectionMode: ConnectionModeConverter.Convert(value.wlanConnectionMode),
			profileName: value.strProfileName,
			ssid: new NetworkIdentifier(value.wlanAssociationAttributes.dot11Ssid),
			bssType: bssType,
			bssid: new NetworkIdentifier(value.wlanAssociationAttributes.dot11Bssid),
			phyType: PhyTypeConverter.Convert(value.wlanAssociationAttributes.dot11PhyType),
			phyIndex: value.wlanAssociationAttributes.uDot11PhyIndex,
			signalQuality: (int)value.wlanAssociationAttributes.wlanSignalQuality,
			rxRate: (int)value.wlanAssociationAttributes.ulRxRate,
			txRate: (int)value.wlanAssociationAttributes.ulTxRate,
			isSecurityEnabled: value.wlanSecurityAttributes.bSecurityEnabled,
			isOneXEnabled: value.wlanSecurityAttributes.bOneXEnabled,
			authenticationAlgorithm: authenticationAlgorithm,
			cipherAlgorithm: cipherAlgorithm));
	}

	/// <summary>
	/// Gets Received Signal Strength Indicator (RSSI) associated with a specified wireless
	/// interface.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <returns>
	/// <para>
	/// result: Action result.
	/// Success if the interface is connected and the function succeeded.
	/// Other if the interface is not connected or the function failed.
	/// </para>
	/// <para>value: RSSI if succeeded</para>
	/// </returns>
	public static (ActionResult result, int value) GetRssi(Guid interfaceId)
	{
		return GetRssi(null, interfaceId);
	}

	internal static (ActionResult result, int value) GetRssi(Base.WlanClient client, Guid interfaceId)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		return Base.GetRssi(container.Content.Handle, interfaceId);
	}

	/// <summary>
	/// Gets wireless connection quality information associated with a specified wireless
	/// interface.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <returns>
	/// <para>
	/// result: Action result.
	/// Success if the interface is connected and the function succeeded.
	/// Other if the interface is not connected or the function failed.
	/// </para>
	/// <para>value: Wireless connection quality information if succeeded</para>
	/// </returns>
	/// <remarks>
	/// This method is supported on Windows 11 (10.0.26100) or newer.
	/// This method does not require location access permissions.
	/// </remarks>
	public static (ActionResult result, RealtimeConnectionQualityInfo value) GetRealtimeConnectionQuality(Guid interfaceId)
	{
		return GetRealtimeConnectionQuality(null, interfaceId);
	}

	internal static (ActionResult result, RealtimeConnectionQualityInfo value) GetRealtimeConnectionQuality(Base.WlanClient client, Guid interfaceId)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		var (result, value) = Base.GetRealtimeConnectionQuality(container.Content.Handle, interfaceId);
		if (result is not ActionResult.Success)
			return (result, null);

		var links = value.LinksInfo.Select(link => new RealtimeConnectionQualityLinkInfo(
			linkId: link.ucLinkID,
			rssi: link.lRssi,
			frequency: (int)link.ulChannelCenterFrequencyMhz,
			bandwidth: (int)link.ulBandwidth));

		return (ActionResult.Success, new RealtimeConnectionQualityInfo(
			phyType: PhyTypeConverter.Convert(value.dot11PhyType),
			linkQuality: (int)value.ulLinkQuality,
			rxRate: (int)value.ulRxRate,
			txRate: (int)value.ulTxRate,
			isMultiLinkOperation: value.bIsMLOConnection,
			links: links));
	}

	#endregion

	#region Enumerate profiles

	/// <summary>
	/// Enumerates wireless profile names in preference order.
	/// </summary>
	/// <returns>Profile names</returns>
	public static IEnumerable<string> EnumerateProfileNames()
	{
		return EnumerateProfileNames(null);
	}

	internal static IEnumerable<string> EnumerateProfileNames(Base.WlanClient client)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		foreach (var interfaceInfo in Base.GetInterfaceInfoList(container.Content.Handle))
		{
			foreach (var profileInfo in Base.GetProfileInfoList(container.Content.Handle, interfaceInfo.InterfaceGuid))
				yield return profileInfo.strProfileName;
		}
	}

	/// <summary>
	/// Enumerates wireless profile information in preference order.
	/// </summary>
	/// <returns>Wireless profile information</returns>
	public static IEnumerable<ProfilePack> EnumerateProfiles()
	{
		return EnumerateProfiles(null);
	}

	internal static IEnumerable<ProfilePack> EnumerateProfiles(Base.WlanClient client)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		foreach (var interfaceInfo in EnumerateInterfaces(container.Content))
		{
			int position = 0;

			foreach (var profileInfo in Base.GetProfileInfoList(container.Content.Handle, interfaceInfo.Id))
			{
				var profileXml = Base.GetProfile(container.Content.Handle, interfaceInfo.Id, profileInfo.strProfileName, out uint profileTypeFlag);
				if (string.IsNullOrWhiteSpace(profileXml))
					continue;

				if (!ProfileTypeConverter.TryConvert(profileTypeFlag, out ProfileType profileType))
					continue;

				yield return new ProfilePack(
					interfaceInfo: interfaceInfo,
					name: profileInfo.strProfileName,
					profileType: profileType,
					profileXml: profileXml,
					position: position++);
			}
		}
	}

	/// <summary>
	/// Enumerates wireless profile and related radio information in preference order.
	/// </summary>
	/// <returns>Wireless profile and related radio information</returns>
	public static IEnumerable<ProfileRadioPack> EnumerateProfileRadios()
	{
		return EnumerateProfileRadios(null);
	}

	internal static IEnumerable<ProfileRadioPack> EnumerateProfileRadios(Base.WlanClient client)
	{
		using var container = new DisposableContainer<Base.WlanClient>(client);

		foreach (var interfaceConnectionInfo in EnumerateInterfaceConnections(container.Content))
		{
			var availableNetworkGroups = EnumerateAvailableNetworkGroups(container.Content, interfaceConnectionInfo)
				.Where(x => !string.IsNullOrWhiteSpace(x.ProfileName))
				.ToArray();

			int position = 0;

			foreach (var profileInfo in Base.GetProfileInfoList(container.Content.Handle, interfaceConnectionInfo.Id))
			{
				var isConnected = interfaceConnectionInfo.IsConnected;
				if (isConnected)
				{
					var (result, value) = Base.GetCurrentConnection(container.Content.Handle, interfaceConnectionInfo.Id);
					isConnected = (result is ActionResult.Success)
						&& string.Equals(profileInfo.strProfileName, value.strProfileName, StringComparison.Ordinal);
				}

				var profileXml = Base.GetProfile(container.Content.Handle, interfaceConnectionInfo.Id, profileInfo.strProfileName, out uint profileTypeFlag);
				if (string.IsNullOrWhiteSpace(profileXml))
					continue;

				if (!ProfileTypeConverter.TryConvert(profileTypeFlag, out ProfileType profileType))
					continue;

				var availableNetworkGroup = availableNetworkGroups.FirstOrDefault(x => string.Equals(x.ProfileName, profileInfo.strProfileName, StringComparison.Ordinal));

				yield return new ProfileRadioPack(
					interfaceInfo: interfaceConnectionInfo,
					name: profileInfo.strProfileName,
					isConnected: isConnected,
					profileType: profileType,
					profileXml: profileXml,
					position: position++,
					phyType: (availableNetworkGroup?.PhyType ?? default),
					signalQuality: (availableNetworkGroup?.SignalQuality ?? 0),
					linkQuality: (availableNetworkGroup?.LinkQuality ?? 0),
					frequency: (availableNetworkGroup?.Frequency ?? 0),
					band: (availableNetworkGroup?.Band ?? 0),
					channel: (availableNetworkGroup?.Channel ?? 0));
			}
		}
	}

	#endregion

	#region Set/Rename/Delete profile

	/// <summary>
	/// Sets (adds or overwrites) the content of a specified wireless profile.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <param name="profileType">Profile type</param>
	/// <param name="profileXml">Profile XML</param>
	/// <param name="profileSecurity">Security descriptor for all-user profile</param>
	/// <param name="overwrite">Whether to overwrite an existing profile</param>
	/// <returns>True if successfully set. False if failed.</returns>
	/// <remarks>
	/// If the content of the profile XML is not valid, a Win32Exception will be thrown.
	/// In such case, check the reason code in the message and see
	/// https://learn.microsoft.com/en-us/windows/win32/nativewifi/wlan-reason-code
	/// https://technet.microsoft.com/en-us/library/3ed3d027-5ae8-4cb0-ade5-0a7c446cd4f7#BKMK_AppndxE
	/// </remarks>
	public static bool SetProfile(Guid interfaceId, ProfileType profileType, string profileXml, string profileSecurity, bool overwrite)
	{
		return SetProfile(null, interfaceId, profileType, profileXml, profileSecurity, overwrite);
	}

	internal static bool SetProfile(Base.WlanClient client, Guid interfaceId, ProfileType profileType, string profileXml, string profileSecurity, bool overwrite)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));
		if (string.IsNullOrWhiteSpace(profileXml))
			throw new ArgumentNullException(nameof(profileXml));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		var profileTypeFlag = ProfileTypeConverter.ConvertBack(profileType);

		return Base.SetProfile(container.Content.Handle, interfaceId, profileTypeFlag, profileXml, profileSecurity, overwrite);
	}

	/// <summary>
	/// Sets (add or overwirte) the user data (credentials) for a specified wireless profile.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <param name="profileName">Profile name</param>
	/// <param name="eapXmlType">EAP XML type</param>
	/// <param name="userDataXml">User data XML</param>
	/// <returns>True if successfully set. False if failed.</returns>
	/// <remarks>
	/// In some cases, this function may return true but fail.
	/// This was observed when setting EapXmlType.AllUsers, but the certificate
	/// referenced in the EAP XML was installed in the users' store.
	/// </remarks>
	public static bool SetProfileEapXmlUserData(Guid interfaceId, string profileName, EapXmlType eapXmlType, string userDataXml)
	{
		return SetProfileEapXmlUserData(null, interfaceId, profileName, eapXmlType, userDataXml);
	}

	internal static bool SetProfileEapXmlUserData(Base.WlanClient client, Guid interfaceId, string profileName, EapXmlType eapXmlType, string userDataXml)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));
		if (string.IsNullOrWhiteSpace(userDataXml))
			throw new ArgumentNullException(nameof(userDataXml));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		var eapXmlTypeFlag = EapXmlTypeConverter.ConvertBack(eapXmlType);

		return Base.SetProfileEapXmlUserData(container.Content.Handle, interfaceId, profileName, eapXmlTypeFlag, userDataXml);
	}

	/// <summary>
	/// Sets the position of a specified wireless profile in preference order.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <param name="profileName">Profile name</param>
	/// <param name="position">Position (starting at 0)</param>
	/// <returns>True if successfully set. False if failed.</returns>
	public static bool SetProfilePosition(Guid interfaceId, string profileName, int position)
	{
		return SetProfilePosition(null, interfaceId, profileName, position);
	}

	internal static bool SetProfilePosition(Base.WlanClient client, Guid interfaceId, string profileName, int position)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));
		if (string.IsNullOrWhiteSpace(profileName))
			throw new ArgumentNullException(nameof(profileName));
		if (position < 0)
			throw new ArgumentOutOfRangeException(nameof(position), "The position must not be negative.");

		using var container = new DisposableContainer<Base.WlanClient>(client);

		return Base.SetProfilePosition(container.Content.Handle, interfaceId, profileName, (uint)position);
	}

	/// <summary>
	/// Renames a specified wireless profile.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <param name="oldProfileName">Old profile name</param>
	/// <param name="newProfileName">New profile name</param>
	/// <returns>True if successfully renamed. False if failed.</returns>
	public static bool RenameProfile(Guid interfaceId, string oldProfileName, string newProfileName)
	{
		return RenameProfile(null, interfaceId, oldProfileName, newProfileName);
	}

	internal static bool RenameProfile(Base.WlanClient client, Guid interfaceId, string oldProfileName, string newProfileName)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));
		if (string.IsNullOrWhiteSpace(oldProfileName))
			throw new ArgumentNullException(nameof(oldProfileName));
		if (string.IsNullOrWhiteSpace(newProfileName))
			throw new ArgumentNullException(nameof(newProfileName));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		return Base.RenameProfile(container.Content.Handle, interfaceId, oldProfileName, newProfileName);
	}

	/// <summary>
	/// Deletes a specified wireless profile.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <param name="profileName">Profile name</param>
	/// <returns>True if successfully deleted. False if failed.</returns>
	public static bool DeleteProfile(Guid interfaceId, string profileName)
	{
		return DeleteProfile(null, interfaceId, profileName);
	}

	internal static bool DeleteProfile(Base.WlanClient client, Guid interfaceId, string profileName)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));
		if (string.IsNullOrWhiteSpace(profileName))
			throw new ArgumentNullException(nameof(profileName));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		return Base.DeleteProfile(container.Content.Handle, interfaceId, profileName);
	}

	#endregion

	#region Connect/Disconnect

	/// <summary>
	/// Attempts to connect to the wireless LAN associated with a specified wireless profile.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <param name="profileName">Profile name</param>
	/// <param name="bssType">BSS network type</param>
	/// <param name="bssid">BSSID of wireless LAN to be connected</param>
	/// <returns>True if successfully requested the connection. False if failed.</returns>
	public static bool ConnectNetwork(Guid interfaceId, string profileName, BssType bssType, NetworkIdentifier bssid = null)
	{
		return ConnectNetwork(null, interfaceId, profileName, bssType, bssid);
	}

	internal static bool ConnectNetwork(Base.WlanClient client, Guid interfaceId, string profileName, BssType bssType, NetworkIdentifier bssid = null)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));
		if (string.IsNullOrWhiteSpace(profileName))
			throw new ArgumentNullException(nameof(profileName));

		var dot11MacAddress = default(DOT11_MAC_ADDRESS);
		if ((bssid is not null) && !DOT11_MAC_ADDRESS.TryCreate(bssid.ToBytes(), out dot11MacAddress))
			throw new ArgumentException("The specified BSSID is invalid", nameof(bssid));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		if (bssid is not null)
		{
			return Base.Connect(container.Content.Handle, interfaceId, profileName, BssTypeConverter.ConvertBack(bssType), dot11MacAddress);
		}
		else
		{
			return Base.Connect(container.Content.Handle, interfaceId, profileName, BssTypeConverter.ConvertBack(bssType));
		}
	}

	/// <summary>
	/// Asynchronously attempts to connect to the wireless LAN associated with a specified wireless
	/// profile.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <param name="profileName">Profile name</param>
	/// <param name="bssType">BSS network type</param>
	/// <param name="timeout">Timeout duration</param>
	/// <returns>True if successfully connected. False if failed or timed out.</returns>
	public static Task<bool> ConnectNetworkAsync(Guid interfaceId, string profileName, BssType bssType, TimeSpan timeout)
	{
		return ConnectNetworkAsync(null, interfaceId, profileName, bssType, null, timeout, CancellationToken.None);
	}

	/// <summary>
	/// Asynchronously attempts to connect to the wireless LAN associated with a specified wireless
	/// profile.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <param name="profileName">Profile name</param>
	/// <param name="bssType">BSS network type</param>
	/// <param name="timeout">Timeout duration</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>True if successfully connected. False if failed or timed out.</returns>
	public static Task<bool> ConnectNetworkAsync(Guid interfaceId, string profileName, BssType bssType, TimeSpan timeout, CancellationToken cancellationToken)
	{
		return ConnectNetworkAsync(null, interfaceId, profileName, bssType, null, timeout, cancellationToken);
	}

	/// <summary>
	/// Asynchronously attempts to connect to the wireless LAN associated with a specified wireless
	/// profile.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <param name="profileName">Profile name</param>
	/// <param name="bssType">BSS network type</param>
	/// <param name="bssid">BSSID of wireless LAN to be connected</param>
	/// <param name="timeout">Timeout duration</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>True if successfully connected. False if failed or timed out.</returns>
	public static Task<bool> ConnectNetworkAsync(Guid interfaceId, string profileName, BssType bssType, NetworkIdentifier bssid, TimeSpan timeout, CancellationToken cancellationToken)
	{
		return ConnectNetworkAsync(null, interfaceId, profileName, bssType, bssid, timeout, cancellationToken);
	}

	internal static async Task<bool> ConnectNetworkAsync(Base.WlanNotificationClient client, Guid interfaceId, string profileName, BssType bssType, NetworkIdentifier bssid, TimeSpan timeout, CancellationToken cancellationToken)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));
		if (string.IsNullOrWhiteSpace(profileName))
			throw new ArgumentNullException(nameof(profileName));

		var dot11MacAddress = default(DOT11_MAC_ADDRESS);
		if ((bssid is not null) && !DOT11_MAC_ADDRESS.TryCreate(bssid.ToBytes(), out dot11MacAddress))
			throw new ArgumentException("The specified BSSID is invalid", nameof(bssid));

		if (timeout <= TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(timeout), "The timeout duration must be positive.");

		using var container = new DisposableContainer<Base.WlanNotificationClient>(client);

		var tcs = new TaskCompletionSource<bool>();

		container.Content.Register(WLAN_NOTIFICATION_SOURCE.WLAN_NOTIFICATION_SOURCE_ACM);
		container.Content.NotificationReceived += (_, data) =>
		{
			if (data.InterfaceGuid != interfaceId)
				return;

			switch ((WLAN_NOTIFICATION_ACM)data.NotificationCode)
			{
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_complete:
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_attempt_fail:
					break;
				default:
					return;
			}

			var connectionNotificationData = Marshal.PtrToStructure<WLAN_CONNECTION_NOTIFICATION_DATA>(data.pData);
			if (connectionNotificationData.strProfileName != profileName)
				return;

			switch ((WLAN_NOTIFICATION_ACM)data.NotificationCode)
			{
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_complete:
					bool isSuccess = (connectionNotificationData.wlanReasonCode is WLAN_REASON_CODE_SUCCESS);
					Task.Run(() => tcs.TrySetResult(isSuccess));
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_attempt_fail:
					// This notification will not always mean that a connection has failed.
					// A connection consists of one or more connection attempts and this notification
					// may be received zero or more times before the connection completes.
					Task.Run(() => tcs.TrySetResult(false));
					break;
			}
		};

		bool result;
		if (bssid is not null)
		{
			result = Base.Connect(container.Content.Handle, interfaceId, profileName, BssTypeConverter.ConvertBack(bssType), dot11MacAddress);
		}
		else
		{
			result = Base.Connect(container.Content.Handle, interfaceId, profileName, BssTypeConverter.ConvertBack(bssType));
		}
		if (!result)
			tcs.SetResult(false);

		using (cancellationToken.Register(() => tcs.TrySetCanceled()))
		{
			var connectTask = tcs.Task;
			var completedTask = await Task.WhenAny(connectTask, Task.Delay(timeout, cancellationToken));

			return (completedTask == connectTask) && connectTask.IsCompleted && connectTask.Result;
		}
	}

	/// <summary>
	/// Disconnects from the wireless LAN associated with a specified wireless interface.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <returns>True if successfully requested the disconnection. False if failed.</returns>
	public static bool DisconnectNetwork(Guid interfaceId)
	{
		return DisconnectNetwork(null, interfaceId);
	}

	internal static bool DisconnectNetwork(Base.WlanClient client, Guid interfaceId)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		return Base.Disconnect(container.Content.Handle, interfaceId);
	}

	/// <summary>
	/// Asynchronously disconnects from the wireless LAN associated with a specified wireless
	/// interface.
	/// </summary>
	/// <param name="interfaceId">Wirelss interface ID</param>
	/// <param name="timeout">Timeout duration</param>
	/// <returns>True if successfully disconnected. False if failed or timed out.</returns>
	public static Task<bool> DisconnectNetworkAsync(Guid interfaceId, TimeSpan timeout)
	{
		return DisconnectNetworkAsync(null, interfaceId, timeout, CancellationToken.None);
	}

	/// <summary>
	/// Asynchronously disconnects from the wireless LAN associated with a specified wireless
	/// interface.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <param name="timeout">Timeout duration</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>True if successfully disconnected. False if failed or timed out.</returns>
	public static Task<bool> DisconnectNetworkAsync(Guid interfaceId, TimeSpan timeout, CancellationToken cancellationToken)
	{
		return DisconnectNetworkAsync(null, interfaceId, timeout, cancellationToken);
	}

	internal static async Task<bool> DisconnectNetworkAsync(Base.WlanNotificationClient client, Guid interfaceId, TimeSpan timeout, CancellationToken cancellationToken)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));
		if (timeout <= TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(timeout), "The timeout duration must be positive.");

		using var container = new DisposableContainer<Base.WlanNotificationClient>(client);

		var tcs = new TaskCompletionSource<bool>();

		container.Content.Register(WLAN_NOTIFICATION_SOURCE.WLAN_NOTIFICATION_SOURCE_ACM);
		container.Content.NotificationReceived += (_, data) =>
		{
			if (data.InterfaceGuid != interfaceId)
				return;

			switch ((WLAN_NOTIFICATION_ACM)data.NotificationCode)
			{
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_disconnected:
					Task.Run(() => tcs.TrySetResult(true));
					break;
			}
		};

		var result = Base.Disconnect(container.Content.Handle, interfaceId);
		if (!result)
			tcs.SetResult(false);

		using (cancellationToken.Register(() => tcs.TrySetCanceled()))
		{
			var disconnectTask = tcs.Task;
			var completedTask = await Task.WhenAny(disconnectTask, Task.Delay(timeout, cancellationToken));

			return (completedTask == disconnectTask) && disconnectTask.IsCompleted && disconnectTask.Result;
		}
	}

	#endregion

	#region Turn on/off

	/// <summary>
	/// Gets radio information of a specified wireless interface.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <returns>Radio information if succeeded. Null if failed.</returns>
	public static RadioInfo GetRadio(Guid interfaceId)
	{
		return GetRadio(null, interfaceId);
	}

	internal static RadioInfo GetRadio(Base.WlanClient client, Guid interfaceId)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		var radioStates = EnumerateRadioStates(container.Content, interfaceId).ToArray();
		if (radioStates is not { Length: > 0 })
			return null;

		return new RadioInfo(radioStates);
	}

	private static IEnumerable<RadioStateSet> EnumerateRadioStates(Base.WlanClient client, Guid interfaceId)
	{
		var capability = Base.GetInterfaceCapability(client.Handle, interfaceId);
		var dot11PhyTypes = capability.dot11PhyTypes /* This value may be null. */ ?? [];

		foreach (var state in Base.GetPhyRadioStates(client.Handle, interfaceId))
		{
			PhyType phyType = default;
			if (state.dwPhyIndex < dot11PhyTypes.Length)
				phyType = PhyTypeConverter.Convert(dot11PhyTypes[state.dwPhyIndex]);

			yield return new RadioStateSet(phyType, state);
		}
	}

	/// <summary>
	/// Turns on the radio of a specified wireless interface (software radio state only).
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <returns>True if successfully changed radio state. False if failed.</returns>
	/// <exception cref="UnauthorizedAccessException">
	/// Thrown when the user is not logged on as a member of Administrators group.
	/// </exception>
	public static bool TurnOnRadio(Guid interfaceId)
	{
		return TurnRadio(null, interfaceId, DOT11_RADIO_STATE.dot11_radio_state_on);
	}

	/// <summary>
	/// Turns off the radio of a specified wireless interface (software radio state only).
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <returns>True if successfully changed radio state. False if failed.</returns>
	/// <exception cref="UnauthorizedAccessException">
	/// Throw when the user is not logged on as a member of Administrators group.
	/// </exception>
	public static bool TurnOffRadio(Guid interfaceId)
	{
		return TurnRadio(null, interfaceId, DOT11_RADIO_STATE.dot11_radio_state_off);
	}

	internal static bool TurnRadio(Base.WlanClient client, Guid interfaceId, DOT11_RADIO_STATE dot11RadioState)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		var state = new WLAN_PHY_RADIO_STATE { dot11SoftwareRadioState = dot11RadioState, };

		return Base.SetPhyRadioState(container.Content.Handle, interfaceId, state);
	}

	#endregion

	#region Auto config

	/// <summary>
	/// Checks if automatic configuration of a specified wireless interface is enabled.
	/// </summary>
	/// <param name="interfaceId">Wireless interface ID</param>
	/// <returns>True if enabled. False if disabled or failed to check.</returns>
	public static bool IsAutoConfig(Guid interfaceId)
	{
		return IsAutoConfig(null, interfaceId);
	}

	internal static bool IsAutoConfig(Base.WlanClient client, Guid interfaceId)
	{
		if (interfaceId == Guid.Empty)
			throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));

		using var container = new DisposableContainer<Base.WlanClient>(client);

		return Base.IsAutoConfig(container.Content.Handle, interfaceId);
	}

	#endregion

	#region Helper

	/// <summary>
	/// Whether to throw an exception when any failure occurs
	/// </summary>
	public static bool ThrowsOnAnyFailure
	{
		get => Base.ThrowsOnAnyFailure;
		set => Base.ThrowsOnAnyFailure = value;
	}

	/// <summary>
	/// Attempts to detect frequency band and channel from center frequency.
	/// </summary>
	/// <param name="frequency">Channel center frequency (KHz)</param>
	/// <param name="band">Frequency band (GHz)</param>
	/// <param name="channel">Channel</param>
	/// <returns>True if successfully detected. False if failed.</returns>
	/// <remarks>
	/// This method is marked as internal for unit test.
	/// As for 5GHz, this method may produce a channel number which is not actually in use.
	/// Some channel numbers of a band overlap those of the other bands. In such cases,
	/// refer frequency band to distinguish them.
	/// </remarks>
	internal static bool TryDetectBandChannel(uint frequency, out float band, out int channel)
	{
		band = 0;
		channel = 0;

		switch (frequency)
		{
			case (>= 2_412_000 and <= 2_484_000):
				{
					// 2.4GHz
					band = 2.4F;

					if (frequency < 2_484_000)
					{
						var gap = frequency / 1_000M - 2_412M; // MHz
						var factor = gap / 5M;
						if (factor - Math.Floor(factor) is 0)
							channel = (int)factor + 1;
					}
					else
					{
						channel = 14;
					}
				}
				break;

			case (>= 3_657_500 and <= 3_692_500):
				{
					// 3.6GHz
					band = 3.6F;

					var gap = frequency / 1_000M - 3_655M; // MHz
					if (gap % 2.5M is 0)
					{
						var factor = gap / 5M;
						channel = (int)Math.Floor(factor) + 131;
					}
				}
				break;

			case (>= 5_170_000 and <= 5_825_000):
				{
					// 5GHz
					band = 5.0F;

					var gap = frequency / 1_000M - 5_170M; // MHz
					var factor = gap / 5M;
					if (factor - Math.Floor(factor) is 0)
						channel = (int)factor + 34;
				}
				break;

			case (>= 5_955_000 and <= 7_115_000):
				{
					// 6GHz
					band = 6.0F;

					var gap = frequency / 1_000M; // MHz
					channel = gap switch
					{
						// 20MHz
						5_955 => 1,
						5_975 => 5,
						5_995 => 9,
						6_015 => 13,
						6_035 => 17,
						6_055 => 21,
						6_075 => 25,
						6_095 => 29,
						6_115 => 33,
						6_135 => 37,
						6_155 => 41,
						6_175 => 45,
						6_195 => 49,
						6_215 => 53,
						6_235 => 57,
						6_255 => 61,
						6_275 => 65,
						6_295 => 69,
						6_315 => 73,
						6_335 => 77,
						6_355 => 81,
						6_375 => 85,
						6_395 => 89,
						6_415 => 93,
						6_435 => 97,
						6_455 => 101,
						6_475 => 105,
						6_495 => 109,
						6_515 => 113,
						6_535 => 117,
						6_555 => 121,
						6_575 => 125,
						6_595 => 129,
						6_615 => 133,
						6_635 => 137,
						6_655 => 141,
						6_675 => 145,
						6_695 => 149,
						6_715 => 153,
						6_735 => 157,
						6_755 => 161,
						6_775 => 165,
						6_795 => 169,
						6_815 => 173,
						6_835 => 177,
						6_855 => 181,
						6_875 => 185,
						6_895 => 189,
						6_915 => 193,
						6_935 => 197,
						6_955 => 201,
						6_975 => 205,
						6_995 => 209,
						7_015 => 213,
						7_035 => 217,
						7_055 => 221,
						7_075 => 225,
						7_095 => 229,
						7_115 => 233,

						// 40MHz
						5_965 => 3,
						6_005 => 11,
						6_045 => 19,
						6_085 => 27,
						6_125 => 35,
						6_165 => 43,
						6_205 => 51,
						6_245 => 59,
						6_285 => 67,
						6_325 => 75,
						6_365 => 83,
						6_405 => 91,
						6_445 => 99,
						6_485 => 107,
						6_525 => 115,
						6_565 => 123,
						6_605 => 131,
						6_645 => 139,
						6_685 => 147,
						6_725 => 155,
						6_765 => 163,
						6_805 => 171,
						6_845 => 179,
						6_885 => 187,
						6_925 => 195,
						6_965 => 203,
						7_005 => 211,
						7_045 => 219,
						7_085 => 227,

						// 80MHz
						5_985 => 7,
						6_065 => 23,
						6_145 => 39,
						6_225 => 55,
						6_305 => 71,
						6_385 => 87,
						6_465 => 103,
						6_545 => 119,
						6_625 => 135,
						6_705 => 151,
						6_785 => 167,
						6_865 => 183,
						6_945 => 199,
						7_025 => 215,

						// 160MHz
						6_025 => 15,
						6_185 => 47,
						6_345 => 79,
						6_505 => 111,
						6_665 => 143,
						6_825 => 175,
						6_985 => 207,

						// 320MHz
						6_105 => 31,
						6_265 => 63,
						6_425 => 95,
						6_585 => 127,
						6_745 => 159,
						6_905 => 191,

						_ => 0
					};
				}
				break;
		}
		return (0 < channel);
	}

	#endregion
}