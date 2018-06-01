using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                return Base.GetInterfaceInfoList(container.Content.Handle)
                    .Select(x => new InterfaceInfo(container.Content.Handle, x));
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
                throw new ArgumentException(nameof(timeout));

            using (var container = new DisposableContainer<Base.WlanNotificationClient>(client))
            {
                var interfaceInfoList = Base.GetInterfaceInfoList(container.Content.Handle);
                var interfaceIds = interfaceInfoList.Select(x => x.InterfaceGuid).ToArray();

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

                var scanTask = tcs.Task;
                await Task.WhenAny(scanTask, Task.Delay(timeout, cancellationToken));

                return counter.Results;
            }
        }

        private class ScanCounter
        {
            private Action _complete;
            private readonly List<Guid> _targets = new List<Guid>();
            private readonly List<Guid> _results = new List<Guid>();

            public IEnumerable<Guid> Results => _results.ToArray();

            public ScanCounter(Action complete, IEnumerable<Guid> targets)
            {
                this._complete = complete;
                this._targets.AddRange(targets);
            }

            private readonly object _locker = new object();

            public void SetSuccess(Guid value)
            {
                lock (_locker)
                {
                    _targets.Remove(value);
                    _results.Add(value);

                    CheckTargets();
                }
            }

            public void SetFailure(Guid value)
            {
                lock (_locker)
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
            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                var interfaceInfoList = Base.GetInterfaceInfoList(container.Content.Handle);

                foreach (var interfaceInfo in interfaceInfoList)
                {
                    var availableNetworkList = Base.GetAvailableNetworkList(container.Content.Handle, interfaceInfo.InterfaceGuid);

                    foreach (var availableNetwork in availableNetworkList)
                    {
                        //Debug.WriteLine("Interface: {0}, SSID: {1}, Signal: {2}",
                        //	interfaceInfo.strInterfaceDescription,
                        //	availableNetwork.dot11Ssid,
                        //	availableNetwork.wlanSignalQuality);

                        yield return new NetworkIdentifier(availableNetwork.dot11Ssid.ToBytes(), availableNetwork.dot11Ssid.ToString());
                    }
                }
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
            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                var interfaceInfoList = Base.GetInterfaceInfoList(container.Content.Handle);

                foreach (var interfaceInfo in interfaceInfoList)
                {
                    var connection = Base.GetConnectionAttributes(container.Content.Handle, interfaceInfo.InterfaceGuid);
                    if (connection.isState != WLAN_INTERFACE_STATE.wlan_interface_state_connected)
                        continue;

                    var association = connection.wlanAssociationAttributes;

                    //Debug.WriteLine("Interface: {0}, SSID: {1}, BSSID: {2}, Signal: {3}",
                    //	interfaceInfo.strInterfaceDescription,
                    //	association.dot11Ssid,
                    //	association.dot11Bssid,
                    //	association.wlanSignalQuality);

                    yield return new NetworkIdentifier(association.dot11Ssid.ToBytes(), association.dot11Ssid.ToString());
                }
            }
        }

        /// <summary>
        /// Enumerates wireless LAN information on available networks.
        /// </summary>
        /// <returns>Wireless LAN information</returns>
        /// <remarks>If multiple profiles are associated with a same network, there will be multiple
        /// entries with the same SSID.</remarks>
        public static IEnumerable<AvailableNetworkPack> EnumerateAvailableNetworks()
        {
            return EnumerateAvailableNetworks(null);
        }

        internal static IEnumerable<AvailableNetworkPack> EnumerateAvailableNetworks(Base.WlanClient client)
        {
            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                var interfaceInfoList = Base.GetInterfaceInfoList(container.Content.Handle);

                foreach (var interfaceInfo in interfaceInfoList.Select(x => new InterfaceInfo(container.Content.Handle,x)))
                {
                    var availableNetworkList = Base.GetAvailableNetworkList(container.Content.Handle, interfaceInfo.Id);
	                foreach (var availableNetwork in availableNetworkList)
                    {
	                    BssType bssType;
	                    if (!BssTypeConverter.TryConvert(availableNetwork.dot11BssType, out bssType))
                            continue;

	                    EncryptionType encryptionType;
	                    if(!EncryptionTypeConverter.TryConvert(availableNetwork.dot11DefaultCipherAlgorithm,out encryptionType))
							continue;

	                    AuthType authType;
	                    if (!AuthTypeConverter.TryConvert(availableNetwork.dot11DefaultAuthAlgorithm, out authType))
							continue;
	                    ;
                        //Debug.WriteLine("Interface: {0}, SSID: {1}, Signal: {2}, Security: {3}",
                        //	interfaceInfo.Description,
                        //	availableNetwork.dot11Ssid,
                        //	availableNetwork.wlanSignalQuality,
                        //	availableNetwork.bSecurityEnabled);

                        yield return new AvailableNetworkPack(
                            interfaceInfo: interfaceInfo,
                            ssid: new NetworkIdentifier(availableNetwork.dot11Ssid.ToBytes(), availableNetwork.dot11Ssid.ToString()),
                            bssType: bssType,
                            signalQuality: (int)availableNetwork.wlanSignalQuality,
                            isSecurityEnabled: availableNetwork.bSecurityEnabled,
                            profileName: availableNetwork.strProfileName,
	                        cipherAlgorithm: encryptionType,
	                        authAlgorithm: authType);
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates wireless LAN information on BSS networks.
        /// </summary>
        /// <returns>Wireless LAN information</returns>
        public static IEnumerable<BssNetworkPack> EnumerateBssNetworks()
        {
            return EnumerateBssNetworks(null);
        }

        internal static IEnumerable<BssNetworkPack> EnumerateBssNetworks(Base.WlanClient client)
        {
            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                var interfaceInfoList = Base.GetInterfaceInfoList(container.Content.Handle);

                foreach (var interfaceInfo in interfaceInfoList.Select(x => new InterfaceInfo(container.Content.Handle, x)))
                {
	                var networkList = EnumerateAvailableNetworks(client);

                    var networkBssEntryList = Base.GetNetworkBssEntryList(container.Content.Handle, interfaceInfo.Id);

                    foreach (var networkBssEntry in networkBssEntryList)
                    {
                        BssType bssType;

                        if (!BssTypeConverter.TryConvert(networkBssEntry.dot11BssType, out bssType))
                            continue;

                        //Debug.WriteLine("Interface: {0}, SSID: {1}, BSSID: {2}, Signal: {3} Link: {4}, Frequency: {5}",
                        //	interfaceInfo.Description,
                        //	networkBssEntry.dot11Ssid,
                        //	networkBssEntry.dot11Bssid,
                        //	networkBssEntry.lRssi,
                        //	networkBssEntry.uLinkQuality,
                        //	networkBssEntry.ulChCenterFrequency);
	                    var network = networkList.Where(o => o.Ssid.ToString() == new NetworkIdentifier(networkBssEntry.dot11Ssid.ToBytes(), networkBssEntry.dot11Ssid.ToString()).ToString());

						//this program work get available network first and get bssnetwork list.
						//so if can not found network. just skip the bss network.
						//it just happen if you don't have a luck.
						if(network.Any()) continue;

                        yield return new BssNetworkPack(
                            interfaceInfo: interfaceInfo,
                            network: network.First(),
                            ssid: new NetworkIdentifier(networkBssEntry.dot11Ssid.ToBytes(), networkBssEntry.dot11Ssid.ToString()),
                            bssType: bssType,
                            bssid: new NetworkIdentifier(networkBssEntry.dot11Bssid.ToBytes(), networkBssEntry.dot11Bssid.ToString()),
                            signalStrength: networkBssEntry.lRssi,
                            linkQuality: (int)networkBssEntry.uLinkQuality,
                            frequency: (int)networkBssEntry.ulChCenterFrequency,
                            channel: DetectChannel(networkBssEntry.ulChCenterFrequency));
                    }
                }
            }
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
            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                var interfaceInfoList = Base.GetInterfaceInfoList(container.Content.Handle);

                foreach (var interfaceInfo in interfaceInfoList)
                {
                    var profileInfoList = Base.GetProfileInfoList(container.Content.Handle, interfaceInfo.InterfaceGuid);

                    foreach (var profileInfo in profileInfoList)
                    {
                        //Debug.WriteLine("Interface: {0}, Profile: {1}",
                        //	interfaceInfo.strInterfaceDescription,
                        //	profileInfo.strProfileName);

                        yield return profileInfo.strProfileName;
                    }
                }
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
        /// <summary>
        /// Enumerates wireless profile information in preference order.
        /// If you want Unprotected Profile, set false
        /// </summary>
        /// <param name="isProtected"></param>
        /// <returns></returns>
        public static IEnumerable<ProfilePack> EnumerateProfiles(bool isProtected = true)
	    {
		    return EnumerateProfiles(null, isProtected);
	    }
        internal static IEnumerable<ProfilePack> EnumerateProfiles(Base.WlanClient client, bool isProtected = true)
        {
            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                var interfaceInfoList = Base.GetInterfaceInfoList(container.Content.Handle);

                foreach (var interfaceInfo in interfaceInfoList.Select(x => new InterfaceInfo(container.Content.Handle, x)))
                {
                    var interfaceIsConnected = (interfaceInfo.State == InterfaceState.Connected);

                    var interfaceIsRadioOn = interfaceIsConnected || IsInterfaceRadioOn(container.Content, interfaceInfo.Id);

                    var availableNetworkList = Base.GetAvailableNetworkList(container.Content.Handle, interfaceInfo.Id)
                        .Where(x => !string.IsNullOrWhiteSpace(x.strProfileName))
                        .ToArray();

                    var connection = Base.GetConnectionAttributes(container.Content.Handle, interfaceInfo.Id);

                    var profileInfoList = Base.GetProfileInfoList(container.Content.Handle, interfaceInfo.Id);

                    int position = 0;

                    foreach (var profileInfo in profileInfoList)
                    {
                        var availableNetwork = availableNetworkList.FirstOrDefault(x => string.Equals(x.strProfileName, profileInfo.strProfileName, StringComparison.Ordinal));
                        var signalQuality = (int)availableNetwork.wlanSignalQuality;
                        var profileIsConnected = interfaceIsConnected && string.Equals(connection.strProfileName, profileInfo.strProfileName, StringComparison.Ordinal);

                        uint profileTypeFlag;
                        ProfileType profileType;

                        var profileXml = isProtected ? Base.GetProfile(container.Content.Handle, interfaceInfo.Id, profileInfo.strProfileName, out profileTypeFlag) : Base.GetProfileUnProtected(container.Content.Handle, interfaceInfo.Id, profileInfo.strProfileName, out profileTypeFlag);
                        if (string.IsNullOrWhiteSpace(profileXml))
                            continue;

                        if (!ProfileTypeConverter.TryConvert(profileTypeFlag, out profileType))
                            continue;

                        //Debug.WriteLine("Interface: {0}, Profile: {1}, Position: {2}, RadioOn: {3}, Signal: {4}, Connected: {5}",
                        //	interfaceInfo.Description,
                        //	profileInfo.strProfileName,
                        //	position,
                        //	interfaceIsRadioOn,
                        //	signalQuality,
                        //	profileIsConnected);

                        yield return new ProfilePack(
                            name: profileInfo.strProfileName,
                            interfaceInfo: interfaceInfo,
                            profileType: profileType,
                            profileXml: profileXml,
                            position: position++,
                            isRadioOn: interfaceIsRadioOn,
                            signalQuality: signalQuality,
                            isConnected: profileIsConnected);
                    }
                }
            }
        }

        private static bool IsInterfaceRadioOn(Base.WlanClient client, Guid interfaceId)
        {
            var states = Base.GetPhyRadioStates(client.Handle, interfaceId);
            if (!states.Any())
                return false;

            var hardwareOn = ConvertToNullableBoolean(states.First().dot11HardwareRadioState);
            var softwareOn = ConvertToNullableBoolean(states.First().dot11SoftwareRadioState);

            return hardwareOn.GetValueOrDefault()
                && softwareOn.GetValueOrDefault();
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
        /// <returns>True if successfully set. False if not.</returns>
        /// <remarks>
        /// If the content of the profile XML is not valid, a Win32Exception will be thrown.
        /// In such case, check the reason code in the message and see
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/ms707394.aspx
        /// https://technet.microsoft.com/en-us/library/3ed3d027-5ae8-4cb0-ade5-0a7c446cd4f7#BKMK_AppndxE
        /// </remarks>
        public static bool SetProfile(Guid interfaceId, ProfileType profileType, string profileXml, string profileSecurity, bool overwrite)
        {
            return SetProfile(null, interfaceId, profileType, profileXml, profileSecurity, overwrite);
        }

        internal static bool SetProfile(Base.WlanClient client, Guid interfaceId, ProfileType profileType, string profileXml, string profileSecurity, bool overwrite)
        {
            if (interfaceId == Guid.Empty)
                throw new ArgumentException(nameof(interfaceId));

            if (string.IsNullOrWhiteSpace(profileXml))
                throw new ArgumentNullException(nameof(profileXml));

            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                var profileTypeFlag = ProfileTypeConverter.ConvertBack(profileType);

                return Base.SetProfile(container.Content.Handle, interfaceId, profileTypeFlag, profileXml, profileSecurity, overwrite);
            }
        }

        /// <summary>
        /// Sets the position of a specified wireless profile in preference order.
        /// </summary>
        /// <param name="interfaceId">Interface ID</param>
        /// <param name="profileName">Profile name</param>
        /// <param name="position">Position (starting at 0)</param>
        /// <returns>True if successfully set. False if not.</returns>
        public static bool SetProfilePosition(Guid interfaceId, string profileName, int position)
        {
            return SetProfilePosition(null, interfaceId, profileName, position);
        }

        internal static bool SetProfilePosition(Base.WlanClient client, Guid interfaceId, string profileName, int position)
        {
            if (interfaceId == Guid.Empty)
                throw new ArgumentException(nameof(interfaceId));

            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentNullException(nameof(profileName));

            if (position < 0)
                throw new ArgumentOutOfRangeException(nameof(position));

            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                return Base.SetProfilePosition(container.Content.Handle, interfaceId, profileName, (uint)position);
            }
        }

        /// <summary>
        /// Renames a specified wireless profile.
        /// </summary>
        /// <param name="interfaceId">Interface ID</param>
        /// <param name="oldProfileName">Old profile name</param>
        /// <param name="newProfileName">New profile name</param>
        /// <returns>True if successfully renamed. False if not.</returns>
        public static bool RenameProfile(Guid interfaceId, string oldProfileName, string newProfileName)
        {
            return RenameProfile(null, interfaceId, oldProfileName, newProfileName);
        }

        internal static bool RenameProfile(Base.WlanClient client, Guid interfaceId, string oldProfileName, string newProfileName)
        {
            if (interfaceId == Guid.Empty)
                throw new ArgumentException(nameof(interfaceId));

            if (string.IsNullOrWhiteSpace(oldProfileName))
                throw new ArgumentNullException(nameof(oldProfileName));

            if (string.IsNullOrWhiteSpace(newProfileName))
                throw new ArgumentNullException(nameof(newProfileName));

            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                return Base.RenameProfile(container.Content.Handle, interfaceId, oldProfileName, newProfileName);
            }
        }

        /// <summary>
        /// Deletes a specified wireless profile.
        /// </summary>
        /// <param name="interfaceId">Interface ID</param>
        /// <param name="profileName">Profile name</param>
        /// <returns>True if successfully deleted. False if could not delete.</returns>
        public static bool DeleteProfile(Guid interfaceId, string profileName)
        {
            return DeleteProfile(null, interfaceId, profileName);
        }

        internal static bool DeleteProfile(Base.WlanClient client, Guid interfaceId, string profileName)
        {
            if (interfaceId == Guid.Empty)
                throw new ArgumentException(nameof(interfaceId));

            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentNullException(nameof(profileName));

            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                return Base.DeleteProfile(container.Content.Handle, interfaceId, profileName);
            }
        }

        #endregion

        #region Connect/Disconnect

        /// <summary>
        /// Attempts to connect to the wireless LAN associated to a specified wireless profile.
        /// </summary>
        /// <param name="interfaceId">Interface ID</param>
        /// <param name="profileName">Profile name</param>
        /// <param name="bssType">BSS network type</param>
        /// <returns>True if successfully requested the connection. False if failed.</returns>
        public static bool ConnectNetwork(Guid interfaceId, string profileName, BssType bssType)
        {
            return ConnectNetwork(null, interfaceId, profileName, bssType);
        }

        internal static bool ConnectNetwork(Base.WlanClient client, Guid interfaceId, string profileName, BssType bssType)
        {
            if (interfaceId == Guid.Empty)
                throw new ArgumentException(nameof(interfaceId));

            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentNullException(nameof(profileName));

            using (var container = new DisposableContainer<Base.WlanClient>(client))
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
            return ConnectNetworkAsync(null, interfaceId, profileName, bssType, timeout, CancellationToken.None);
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
            return ConnectNetworkAsync(null, interfaceId, profileName, bssType, timeout, cancellationToken);
        }

        internal static async Task<bool> ConnectNetworkAsync(Base.WlanNotificationClient client, Guid interfaceId, string profileName, BssType bssType, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (interfaceId == Guid.Empty)
                throw new ArgumentException(nameof(interfaceId));

            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentNullException(nameof(profileName));

            if (timeout <= TimeSpan.Zero)
                throw new ArgumentException(nameof(timeout));

            using (var container = new DisposableContainer<Base.WlanNotificationClient>(client))
            {
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
                            Task.Run(() => tcs.TrySetResult(true));
                            break;
                        case WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_attempt_fail:
                            // This notification will not always mean that a connection has failed.
                            // A connection consists of one or more connection attempts and this notification
                            // may be received zero or more times before the connection completes.
                            Task.Run(() => tcs.TrySetResult(false));
                            break;
                    }
                };

                var result = Base.Connect(container.Content.Handle, interfaceId, profileName, BssTypeConverter.ConvertBack(bssType));
                if (!result)
                    tcs.SetResult(false);

                var connectTask = tcs.Task;
                var completedTask = await Task.WhenAny(connectTask, Task.Delay(timeout, cancellationToken));

                return (completedTask == connectTask) && connectTask.Result;
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
                throw new ArgumentException(nameof(interfaceId));

            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                return Base.Disconnect(container.Content.Handle, interfaceId);
            }
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
                throw new ArgumentException(nameof(interfaceId));

            if (timeout <= TimeSpan.Zero)
                throw new ArgumentException(nameof(timeout));

            using (var container = new DisposableContainer<Base.WlanNotificationClient>(client))
            {
                var tcs = new TaskCompletionSource<bool>();

                container.Content.NotificationReceived += (sender, data) =>
                {
                    if (data.InterfaceGuid != interfaceId)
                        return;

                    switch ((WLAN_NOTIFICATION_ACM)data.NotificationCode)
                    {
                        case WLAN_NOTIFICATION_ACM.wlan_notification_acm_disconnected:
                            Task.Run(() => tcs.TrySetResult(true), cancellationToken);
                            break;
                    }
                };

                var result = Base.Disconnect(container.Content.Handle, interfaceId);
                if (!result)
                    tcs.SetResult(false);

                var disconnectTask = tcs.Task;
                var completedTask = await Task.WhenAny(disconnectTask, Task.Delay(timeout, cancellationToken));

                return (completedTask == disconnectTask) && disconnectTask.Result;
            }
        }

        #endregion

        #region Turn on/off

        /// <summary>
        /// Gets wireless interface radio information of a specified wireless interface.
        /// </summary>
        /// <param name="interfaceId">Interface ID</param>
        /// <returns>Wireless interface radio information if succeeded. Null if not.</returns>
        public static InterfaceRadio GetInterfaceRadio(Guid interfaceId)
        {
            return GetInterfaceRadio(null, interfaceId);
        }

        internal static InterfaceRadio GetInterfaceRadio(Base.WlanClient client, Guid interfaceId)
        {
            if (interfaceId == Guid.Empty)
                throw new ArgumentException(nameof(interfaceId));

            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                var capability = Base.GetInterfaceCapability(container.Content.Handle, interfaceId);
                var states = Base.GetPhyRadioStates(container.Content.Handle, interfaceId); // The underlying collection is array.

                if ((capability.interfaceType == WLAN_INTERFACE_TYPE.wlan_interface_type_invalid) ||
                    (capability.dwNumberOfSupportedPhys != states.Count()))
                    return null;

                var radioSets = Enumerable.Zip(
                    capability.dot11PhyTypes,
                    states.OrderBy(x => x.dwPhyIndex),
                    (x, y) => new RadioSet(
                        type: PhyTypeConverter.Convert(x),
                        softwareOn: ConvertToNullableBoolean(y.dot11SoftwareRadioState),
                        hardwareOn: ConvertToNullableBoolean(y.dot11HardwareRadioState)));

                return new InterfaceRadio(
                    id: interfaceId,
                    radioSets: radioSets);
            }
        }

        /// <summary>
        /// Turns on the radio of a specified wireless interface (software radio state only).
        /// </summary>
        /// <param name="interfaceId">Interface ID</param>
        /// <returns>True if successfully changed radio state. False if not.</returns>
        /// <remarks>
        /// If the user is not logged on as a member of Administrators group,
        /// an UnauthorizedAccessException should be thrown.
        /// </remarks>
        public static bool TurnOnInterfaceRadio(Guid interfaceId)
        {
            return TurnInterfaceRadio(null, interfaceId, DOT11_RADIO_STATE.dot11_radio_state_on);
        }

        /// <summary>
        /// Turns off the radio of a specified wireless interface (software radio state only).
        /// </summary>
        /// <param name="interfaceId">Interface ID</param>
        /// <returns>True if successfully changed radio state. False if not.</returns>
        /// <remarks>
        /// If the user is not logged on as a member of Administrators group,
        /// an UnauthorizedAccessException should be thrown.
        /// </remarks>
        public static bool TurnOffInterfaceRadio(Guid interfaceId)
        {
            return TurnInterfaceRadio(null, interfaceId, DOT11_RADIO_STATE.dot11_radio_state_off);
        }

        internal static bool TurnInterfaceRadio(Base.WlanClient client, Guid interfaceId, DOT11_RADIO_STATE radioState)
        {
            if (interfaceId == Guid.Empty)
                throw new ArgumentException(nameof(interfaceId));

            using (var container = new DisposableContainer<Base.WlanClient>(client))
            {
                var phyRadioState = new WLAN_PHY_RADIO_STATE { dot11SoftwareRadioState = radioState, };

                return Base.SetPhyRadioState(container.Content.Handle, interfaceId, phyRadioState);
            }
        }

        #endregion

        #region Helper

        private static bool? ConvertToNullableBoolean(DOT11_RADIO_STATE source)
        {
            switch (source)
            {
                case DOT11_RADIO_STATE.dot11_radio_state_on:
                    return true;
                case DOT11_RADIO_STATE.dot11_radio_state_off:
                    return false;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Detects wireless LAN channel from center frequency.
        /// </summary>
        /// <param name="frequency">Center frequency (KHz)</param>
        /// <returns>Channel number if successfully detected. 0 if not.</returns>
        /// <remarks>
        /// This method is marked as internal for unit test.
        /// As for 5GHz, this method may produce a channel number which is not actually in use.
        /// Also, some channel numbers of 5GHz overlap those of 3.6GHz. In such cases, refer
        /// the frequency to distinguish them.
        /// </remarks>
        internal static int DetectChannel(uint frequency)
        {
            // 2.4GHz
            if ((2412000 <= frequency) && (frequency < 2484000))
            {
                var gap = frequency / 1000M - 2412M; // MHz
                var factor = gap / 5M;
                if (factor - Math.Floor(factor) == 0)
                    return (int)factor + 1;
            }
            if (frequency == 2484000)
                return 14;

            // 3.6GHz
            if ((3657500 <= frequency) && (frequency <= 3692500))
            {
                var gap = frequency / 1000M - 3655M; // MHz
                if (gap % 2.5M == 0)
                {
                    var factor = gap / 5M;
                    return (int)Math.Floor(factor) + 131;
                }
            }

            // 5GHz
            if ((5170000 <= frequency) && (frequency <= 5825000))
            {
                var gap = frequency / 1000M - 5170M; // MHz
                var factor = gap / 5M;
                if (factor - Math.Floor(factor) == 0)
                    return (int)factor + 34;
            }

            return 0;
        }

        #endregion
    }
}