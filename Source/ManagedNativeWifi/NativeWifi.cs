using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ManagedNativeWifi.Common;
using static ManagedNativeWifi.Win32.NativeMethod;
using Base = ManagedNativeWifi.Win32.BaseMethod;

namespace ManagedNativeWifi
{
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
				var connection = Base.GetConnectionAttributes(container.Content.Handle, interfaceInfo.InterfaceGuid);

				var isConnected = (interfaceInfo.isState == WLAN_INTERFACE_STATE.wlan_interface_state_connected);
				var isRadioOn = isConnected ||
					EnumerateInterfaceRadioSets(container.Content, interfaceInfo.InterfaceGuid).Any(x => x.On.GetValueOrDefault());

				yield return new InterfaceConnectionInfo(
					interfaceInfo,
					connectionMode: ConnectionModeConverter.Convert(connection.wlanConnectionMode),
					isRadioOn: isRadioOn,
					isConnected: isConnected,
					profileName: connection.strProfileName);
			}
		}

		#endregion

		#region Scan networks

		/// <summary>
		/// Asynchronously requests wireless interfaces to scan wireless LANs.
		/// </summary>
		/// <param name="timeout">Timeout duration</param>
		/// <returns>Interface IDs that successfully scanned</returns>
		public static Task<IEnumerable<Guid>> ScanNetworksAsync(TimeSpan timeout)
		{
			return ScanNetworksAsync(null, timeout, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously requests wireless interfaces to scan wireless LANs.
		/// </summary>
		/// <param name="timeout">Timeout duration</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Interface IDs that successfully scanned</returns>
		public static Task<IEnumerable<Guid>> ScanNetworksAsync(TimeSpan timeout, CancellationToken cancellationToken)
		{
			return ScanNetworksAsync(null, timeout, cancellationToken);
		}

		internal static async Task<IEnumerable<Guid>> ScanNetworksAsync(Base.WlanNotificationClient client, TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (timeout <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(timeout), "The timeout duration must be positive.");

			using var container = new DisposableContainer<Base.WlanNotificationClient>(client);

			var interfaceIds = Base.GetInterfaceInfoList(container.Content.Handle)
				.Select(x => x.InterfaceGuid)
				.ToArray();

			var tcs = new TaskCompletionSource<bool>();
			var counter = new ScanCounter(() => Task.Run(() => tcs.TrySetResult(true)), interfaceIds);

			container.Content.NotificationReceived += (sender, data) =>
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

			foreach (var interfaceId in interfaceIds)
			{
				var result = Base.Scan(container.Content.Handle, interfaceId);
				if (!result)
					counter.SetFailure(interfaceId);
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
			private readonly List<Guid> _targets = new List<Guid>();
			private readonly List<Guid> _results = new List<Guid>();

			public IEnumerable<Guid> Results => _results.ToArray();

			public ScanCounter(Action complete, IEnumerable<Guid> targets)
			{
				this._complete = complete;
				this._targets.AddRange(targets);
			}

			private readonly object _lock = new object();

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
				foreach (var availableNetwork in Base.GetAvailableNetworkList(container.Content.Handle, interfaceInfo.InterfaceGuid))
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
				var connection = Base.GetConnectionAttributes(container.Content.Handle, interfaceInfo.InterfaceGuid);
				if (connection.isState != WLAN_INTERFACE_STATE.wlan_interface_state_connected)
					continue;

				var association = connection.wlanAssociationAttributes;

				yield return new NetworkIdentifier(association.dot11Ssid);
			}
		}

		/// <summary>
		/// Enumerates Association Attributess of connected wireless LANs. 
		/// </summary>
		/// <returns>Association Attributes</returns>
		public static IEnumerable<AssociationAttributes> EnumerateAssociationAttributes()
		{
			return EnumerateAssociationAttributes(null);
		}

		internal static IEnumerable<AssociationAttributes> EnumerateAssociationAttributes(Base.WlanClient client)
		{
			using var container = new DisposableContainer<Base.WlanClient>(client);

			foreach (var interfaceInfo in Base.GetInterfaceInfoList(container.Content.Handle))
			{
				var connection = Base.GetConnectionAttributes(container.Content.Handle, interfaceInfo.InterfaceGuid);
				var attributes = connection.wlanAssociationAttributes;

				var ssid = attributes.dot11Ssid.ToString();
				var bssType = attributes.dot11BssType.ToString();
				var bssid = attributes.dot11Bssid.ToString();
				var phyType = attributes.dot11PhyType.ToString();
				var phyIndex = attributes.uDot11PhyIndex;
				var signalQuality = attributes.wlanSignalQuality;
				var rxRate = attributes.ulRxRate;
				var txRate = attributes.ulTxRate;

				yield return new AssociationAttributes(ssid, bssType, bssid, phyType, phyIndex, signalQuality, rxRate, txRate);
			}
		}
		
		/// <summary>
		/// Enumerates wireless LAN information on available networks.
		/// </summary>
		/// <returns>Wireless LAN information on available networks</returns>
		/// <remarks>
		/// If multiple profiles are associated with a same network, there will be multiple entries with
		/// the same SSID.
		/// </remarks>
		public static IEnumerable<AvailableNetworkPack> EnumerateAvailableNetworks()
		{
			return EnumerateAvailableNetworks(null);
		}

		internal static IEnumerable<AvailableNetworkPack> EnumerateAvailableNetworks(Base.WlanClient client)
		{
			using var container = new DisposableContainer<Base.WlanClient>(client);

			foreach (var interfaceInfo in EnumerateInterfaces(container.Content))
			{
				foreach (var availableNetwork in Base.GetAvailableNetworkList(container.Content.Handle, interfaceInfo.Id))
				{
					if (!BssTypeConverter.TryConvert(availableNetwork.dot11BssType, out BssType bssType))
						continue;

					if (!AuthenticationAlgorithmConverter.TryConvert(availableNetwork.dot11DefaultAuthAlgorithm, out AuthenticationAlgorithm authenticationAlgorithm))
						continue;

					if (!CipherAlgorithmConverter.TryConvert(availableNetwork.dot11DefaultCipherAlgorithm, out CipherAlgorithm cipherAlgorithm))
						continue;

					yield return new AvailableNetworkPack(
						interfaceInfo: interfaceInfo,
						ssid: new NetworkIdentifier(availableNetwork.dot11Ssid),
						bssType: bssType,
						signalQuality: (int)availableNetwork.wlanSignalQuality,
						isSecurityEnabled: availableNetwork.bSecurityEnabled,
						profileName: availableNetwork.strProfileName,
						authenticationAlgorithm: authenticationAlgorithm,
						cipherAlgorithm: cipherAlgorithm);
				}
			}
		}

		/// <summary>
		/// Enumerates wireless LAN information on available networks and group of associated BSS networks.
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
			foreach (var availableNetwork in Base.GetAvailableNetworkList(client.Handle, interfaceInfo.Id))
			{
				if (!BssTypeConverter.TryConvert(availableNetwork.dot11BssType, out BssType bssType))
					continue;

				if (!AuthenticationAlgorithmConverter.TryConvert(availableNetwork.dot11DefaultAuthAlgorithm, out AuthenticationAlgorithm authenticationAlgorithm))
					continue;

				if (!CipherAlgorithmConverter.TryConvert(availableNetwork.dot11DefaultCipherAlgorithm, out CipherAlgorithm cipherAlgorithm))
					continue;

				var bssNetworks = Base.GetNetworkBssEntryList(client.Handle, interfaceInfo.Id,
					availableNetwork.dot11Ssid, availableNetwork.dot11BssType, availableNetwork.bSecurityEnabled)
					.Select(x => TryConvertBssNetwork(interfaceInfo, x, out BssNetworkPack bssNetwork) ? bssNetwork : null)
					.Where(x => x is not null);

				yield return new AvailableNetworkGroupPack(
					interfaceInfo: interfaceInfo,
					ssid: new NetworkIdentifier(availableNetwork.dot11Ssid),
					bssType: bssType,
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
			return EnumerateBssNetworks(null);
		}

		internal static IEnumerable<BssNetworkPack> EnumerateBssNetworks(Base.WlanClient client)
		{
			using var container = new DisposableContainer<Base.WlanClient>(client);

			foreach (var interfaceInfo in EnumerateInterfaces(container.Content))
			{
				foreach (var networkBssEntry in Base.GetNetworkBssEntryList(container.Content.Handle, interfaceInfo.Id))
				{
					if (TryConvertBssNetwork(interfaceInfo, networkBssEntry, out BssNetworkPack bssNetwork))
						yield return bssNetwork;
				}
			}
		}

		private static bool TryConvertBssNetwork(InterfaceInfo interfaceInfo, WLAN_BSS_ENTRY bssEntry, out BssNetworkPack bssNetwork)
		{
			bssNetwork = null;

			if (!BssTypeConverter.TryConvert(bssEntry.dot11BssType, out BssType bssType))
				return false;

			if (!TryDetectBandChannel(bssEntry.ulChCenterFrequency, out float band, out int channel))
				return false;

			bssNetwork = new BssNetworkPack(
				interfaceInfo: interfaceInfo,
				ssid: new NetworkIdentifier(bssEntry.dot11Ssid),
				bssType: bssType,
				bssid: new NetworkIdentifier(bssEntry.dot11Bssid),
				phyType: PhyTypeConverter.Convert(bssEntry.dot11BssPhyType),
				signalStrength: bssEntry.lRssi,
				linkQuality: (int)bssEntry.uLinkQuality,
				frequency: (int)bssEntry.ulChCenterFrequency,
				band: band,
				channel: channel);
			return true;
		}

		#endregion

		#region Enumerate profiles

		/// <summary>
		/// Enumerates wireless profile names in preference order.
		/// </summary>
		/// <returns>Wireless profile names</returns>
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
						name: profileInfo.strProfileName,
						interfaceInfo: interfaceInfo,
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
					var profileXml = Base.GetProfile(container.Content.Handle, interfaceConnectionInfo.Id, profileInfo.strProfileName, out uint profileTypeFlag);
					if (string.IsNullOrWhiteSpace(profileXml))
						continue;

					if (!ProfileTypeConverter.TryConvert(profileTypeFlag, out ProfileType profileType))
						continue;

					var availableNetworkGroup = availableNetworkGroups.FirstOrDefault(x => string.Equals(x.ProfileName, profileInfo.strProfileName, StringComparison.Ordinal));

					yield return new ProfileRadioPack(
						name: profileInfo.strProfileName,
						interfaceInfo: interfaceConnectionInfo,
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
		/// <param name="interfaceId">Interface ID</param>
		/// <param name="profileType">Profile type</param>
		/// <param name="profileXml">Profile XML</param>
		/// <param name="profileSecurity">Security descriptor for all-user profile</param>
		/// <param name="overwrite">Whether to overwrite an existing profile</param>
		/// <returns>True if successfully set. False if failed.</returns>
		/// <remarks>
		/// If the content of the profile XML is not valid, a Win32Exception will be thrown.
		/// In such case, check the reason code in the message and see
		/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/wlan-reason-code
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
		/// <param name="interfaceId">Interface ID</param>
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
				throw new ArgumentException(nameof(interfaceId));

			if (string.IsNullOrWhiteSpace(userDataXml))
				throw new ArgumentNullException(nameof(userDataXml));

			using var container = new DisposableContainer<Base.WlanClient>(client);

			var eapXmlTypeFlag = EapXmlTypeConverter.ConvertBack(eapXmlType);

			return Base.SetProfileEapXmlUserData(container.Content.Handle, interfaceId, profileName, eapXmlTypeFlag, userDataXml);
		}

		/// <summary>
		/// Sets the position of a specified wireless profile in preference order.
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
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
		/// <param name="interfaceId">Interface ID</param>
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
		/// <param name="interfaceId">Interface ID</param>
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
		/// Attempts to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
		/// <param name="profileName">Profile name</param>
		/// <param name="bssType">BSS network type</param>
		/// <param name="bssid">BSSID of the network</param>
		/// <returns>True if successfully requested the connection. False if failed.</returns>
		public static bool ConnectNetwork(Guid interfaceId, string profileName, BssType bssType, PhysicalAddress bssid = null)
		{
			return ConnectNetwork(null, interfaceId, profileName, bssType, bssid);
		}

		internal static bool ConnectNetwork(Base.WlanClient client, Guid interfaceId, string profileName, BssType bssType, PhysicalAddress bssid = null)
		{
			if (interfaceId == Guid.Empty)
				throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));
			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			using var container = new DisposableContainer<Base.WlanClient>(client);

			if (bssid is not null)
			{
				var dot11MacAddress = new DOT11_MAC_ADDRESS { ucDot11MacAddress = bssid.GetAddressBytes() };
				return Base.Connect(container.Content.Handle, interfaceId, profileName, BssTypeConverter.ConvertBack(bssType), dot11MacAddress);
			}
			else
			{
				return Base.Connect(container.Content.Handle, interfaceId, profileName, BssTypeConverter.ConvertBack(bssType));
			}
		}

		/// <summary>
		/// Asynchronously attempts to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
		/// <param name="profileName">Profile name</param>
		/// <param name="bssType">BSS network type</param>
		/// <param name="timeout">Timeout duration</param>
		/// <returns>True if successfully connected. False if failed or timed out.</returns>
		public static Task<bool> ConnectNetworkAsync(Guid interfaceId, string profileName, BssType bssType, TimeSpan timeout)
		{
			return ConnectNetworkAsync(null, interfaceId, profileName, bssType, null, timeout, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously attempts to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
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
		/// Asynchronously attempts to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
		/// <param name="profileName">Profile name</param>
		/// <param name="bssType">BSS network type</param>
		/// <param name="bssid">BSSID of the network</param>
		/// <param name="timeout">Timeout duration</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>True if successfully connected. False if failed or timed out.</returns>
		public static Task<bool> ConnectNetworkAsync(Guid interfaceId, string profileName, BssType bssType, PhysicalAddress bssid, TimeSpan timeout, CancellationToken cancellationToken)
		{
			return ConnectNetworkAsync(null, interfaceId, profileName, bssType, bssid, timeout, cancellationToken);
		}

		internal static async Task<bool> ConnectNetworkAsync(Base.WlanNotificationClient client, Guid interfaceId, string profileName, BssType bssType, PhysicalAddress bssid, TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (interfaceId == Guid.Empty)
				throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));
			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));
			if (timeout <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(timeout), "The timeout duration must be positive.");

			using var container = new DisposableContainer<Base.WlanNotificationClient>(client);

			var tcs = new TaskCompletionSource<bool>();

			container.Content.NotificationReceived += (sender, data) =>
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
						bool isSuccess = (connectionNotificationData.wlanReasonCode == WLAN_REASON_CODE_SUCCESS);
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
				var dot11MacAddress = new DOT11_MAC_ADDRESS { ucDot11MacAddress = bssid.GetAddressBytes() };
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
		/// Disconnects from the wireless LAN associated to a specified wireless interface.
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
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
		/// Asynchronously disconnects from the wireless LAN associated to a specified wireless interface.
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
		/// <param name="timeout">Timeout duration</param>
		/// <returns>True if successfully disconnected. False if failed or timed out.</returns>
		public static Task<bool> DisconnectNetworkAsync(Guid interfaceId, TimeSpan timeout)
		{
			return DisconnectNetworkAsync(null, interfaceId, timeout, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously disconnects from the wireless LAN associated to a specified wireless interface.
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
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

			container.Content.NotificationReceived += (sender, data) =>
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
		/// Gets wireless interface radio information of a specified wireless interface.
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
		/// <returns>Wireless interface radio information if succeeded. Null if failed.</returns>
		public static RadioInfo GetInterfaceRadio(Guid interfaceId)
		{
			return GetInterfaceRadio(null, interfaceId);
		}

		internal static RadioInfo GetInterfaceRadio(Base.WlanClient client, Guid interfaceId)
		{
			if (interfaceId == Guid.Empty)
				throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));

			using var container = new DisposableContainer<Base.WlanClient>(client);

			var radioSets = EnumerateInterfaceRadioSets(container.Content, interfaceId).ToArray();
			if (!radioSets.Any())
				return null;

			return new RadioInfo(
				id: interfaceId,
				radioSets: radioSets);
		}

		private static IEnumerable<RadioSet> EnumerateInterfaceRadioSets(Base.WlanClient client, Guid interfaceId)
		{
			var capability = Base.GetInterfaceCapability(client.Handle, interfaceId);
			var states = Base.GetPhyRadioStates(client.Handle, interfaceId); // The underlying collection is array.

			if ((capability.interfaceType == WLAN_INTERFACE_TYPE.wlan_interface_type_invalid) ||
				(capability.dwNumberOfSupportedPhys != states.Count()) ||
				(capability.dot11PhyTypes?.Any() is not true)) // This value may be null.
				return Enumerable.Empty<RadioSet>();

			return Enumerable.Zip(
				capability.dot11PhyTypes,
				states.OrderBy(x => x.dwPhyIndex),
				(x, y) => new RadioSet(
					type: PhyTypeConverter.Convert(x),
					hardwareOn: ConvertToNullableBoolean(y.dot11HardwareRadioState),
					softwareOn: ConvertToNullableBoolean(y.dot11SoftwareRadioState)));

			static bool? ConvertToNullableBoolean(DOT11_RADIO_STATE source)
			{
				return source switch
				{
					DOT11_RADIO_STATE.dot11_radio_state_on => true,
					DOT11_RADIO_STATE.dot11_radio_state_off => false,
					_ => (bool?)null,
				};
			}
		}

		/// <summary>
		/// Turns on the radio of a specified wireless interface (software radio state only).
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
		/// <returns>True if successfully changed radio state. False if failed.</returns>
		/// <exception cref="UnauthorizedAccessException">
		/// If the user is not logged on as a member of Administrators group.
		/// </exception>
		public static bool TurnOnInterfaceRadio(Guid interfaceId)
		{
			return TurnInterfaceRadio(null, interfaceId, DOT11_RADIO_STATE.dot11_radio_state_on);
		}

		/// <summary>
		/// Turns off the radio of a specified wireless interface (software radio state only).
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
		/// <returns>True if successfully changed radio state. False if failed.</returns>
		/// <exception cref="UnauthorizedAccessException">
		/// If the user is not logged on as a member of Administrators group.
		/// </exception>
		public static bool TurnOffInterfaceRadio(Guid interfaceId)
		{
			return TurnInterfaceRadio(null, interfaceId, DOT11_RADIO_STATE.dot11_radio_state_off);
		}

		internal static bool TurnInterfaceRadio(Base.WlanClient client, Guid interfaceId, DOT11_RADIO_STATE radioState)
		{
			if (interfaceId == Guid.Empty)
				throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));

			using var container = new DisposableContainer<Base.WlanClient>(client);

			var phyRadioState = new WLAN_PHY_RADIO_STATE { dot11SoftwareRadioState = radioState, };

			return Base.SetPhyRadioState(container.Content.Handle, interfaceId, phyRadioState);
		}

		#endregion

		#region Auto config

		/// <summary>
		/// Checks if automatic configuration of a specified wireless interface is enabled.
		/// </summary>
		/// <param name="interfaceId">Interface ID</param>
		/// <returns>True if enabled. False if disabled or failed to check.</returns>
		public static bool IsInterfaceAutoConfig(Guid interfaceId)
		{
			return IsInterfaceAutoConfig(null, interfaceId);
		}

		internal static bool IsInterfaceAutoConfig(Base.WlanClient client, Guid interfaceId)
		{
			if (interfaceId == Guid.Empty)
				throw new ArgumentException("The specified interface ID is invalid.", nameof(interfaceId));

			using var container = new DisposableContainer<Base.WlanClient>(client);

			return Base.IsAutoConfig(container.Content.Handle, interfaceId).GetValueOrDefault();
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
		/// <param name="frequency">Center frequency (KHz)</param>
		/// <param name="band">Frequency band (GHz)</param>
		/// <param name="channel">Channel</param>
		/// <returns>True if successfully detected. False if failed.</returns>
		/// <remarks>
		/// This method is marked as internal for unit test.
		/// As for 5GHz, this method may produce a channel number which is not actually in use.
		/// Some channel numbers of 5GHz overlap those of 3.6GHz. In such cases, refer frequency band
		/// to distinguish them.
		/// </remarks>
		internal static bool TryDetectBandChannel(uint frequency, out float band, out int channel)
		{
			band = 0;
			channel = 0;

			if (frequency is (>= 2_412_000 and <= 2_484_000))
			{
				// 2.4GHz
				band = 2.4F;

				if (frequency < 2_484_000)
				{
					var gap = frequency / 1_000M - 2_412M; // MHz
					var factor = gap / 5M;
					if (factor - Math.Floor(factor) == 0)
						channel = (int)factor + 1;
				}
				else
				{
					channel = 14;
				}
			}
			else if (frequency is (>= 3_657_500 and <= 3_692_500))
			{
				// 3.6GHz
				band = 3.6F;

				var gap = frequency / 1_000M - 3_655M; // MHz
				if (gap % 2.5M == 0)
				{
					var factor = gap / 5M;
					channel = (int)Math.Floor(factor) + 131;
				}
			}
			else if (frequency is (>= 5_170_000 and <= 5_825_000))
			{
				// 5GHz
				band = 5.0F;

				var gap = frequency / 1_000M - 5_170M; // MHz
				var factor = gap / 5M;
				if (factor - Math.Floor(factor) == 0)
					channel = (int)factor + 34;
			}

			return (0 < channel);
		}

		#endregion
	}
}