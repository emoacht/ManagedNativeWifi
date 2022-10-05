using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ManagedNativeWifi.Args;
using static ManagedNativeWifi.Win32.NativeMethod;

using Base = ManagedNativeWifi.Win32.BaseMethod;

/*
 * 2022/10/04 : Changes by C. Pohlmann to included more meaningful information in event.
 */

namespace ManagedNativeWifi
{
	/// <summary>
	/// An instantiatable implementation of Native Wifi API
	/// </summary>
	public class NativeWifiPlayer : IDisposable
	{
		private readonly Base.WlanNotificationClient _client;

		/// <summary>
		/// Constructor
		/// </summary>
		public NativeWifiPlayer()
		{
			_client = new Base.WlanNotificationClient();
			_client.NotificationReceived += OnNotificationReceived;
		}

		/// <summary>
		/// Occurs when wireless LAN list is refreshed.
		/// </summary>
		public event EventHandler NetworkRefreshed;

		/// <summary>
		/// Occurs when availability of a wireless interface is changed.
		/// </summary>
		public event EventHandler<AvailabilityArgs> AvailabilityChanged; // 2022/10/04

		/// <summary>
		/// Occurs when a wireless interface is added/removed/enabled/disabled
		/// </summary>
		public event EventHandler<InterfaceArgs> InterfaceChanged; // 2022/10/04

		/// <summary>
		/// Occurs when connection of a wireless interface is changed.
		/// </summary>
		public event EventHandler<ConnectionArgs> ConnectionChanged; // 2022/10/04

		/// <summary>
		/// Occurs when a wireless profile or wireless profile name is changed.
		/// </summary>
		public event EventHandler<ProfileArgs> ProfileChanged; // 2022/10/04

		private void OnNotificationReceived(object sender, WLAN_NOTIFICATION_DATA e)
		{
			Debug.WriteLine($"NotificationReceived: {(WLAN_NOTIFICATION_ACM)e.NotificationCode}");

			// 2022/10/04 >>
			var i = NativeWifi.EnumerateInterfaces().Where(a => a.Id.Equals(e.InterfaceGuid)).FirstOrDefault();
			// 2022/10/04 <<

			//https://learn.microsoft.com/en-us/windows/win32/api/wlanapi/ne-wlanapi-wlan_notification_acm-r1

			switch ((WLAN_NOTIFICATION_ACM)e.NotificationCode)
			{
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_scan_list_refresh:
					NetworkRefreshed?.Invoke(this, EventArgs.Empty);
					break;
				// 2022/10/04 >>
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_network_available:
					AvailabilityChanged?.Invoke(this, new AvailabilityArgs() { State = EnumAvailability.Available, Interface = i });
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_network_not_available:
					AvailabilityChanged?.Invoke(this, new AvailabilityArgs() { State = EnumAvailability.Unavailable , Interface = i});
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_start:
					AvailabilityChanged?.Invoke(this, new AvailabilityArgs() { State = EnumAvailability.Start , Interface = i});
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_end:
					AvailabilityChanged?.Invoke(this, new AvailabilityArgs() { State = EnumAvailability.End , Interface = i});
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_interface_arrival:
					InterfaceChanged?.Invoke(this, new InterfaceArgs() { State = EnumInterfaceState.Arrival, Interface = i});
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_interface_removal:
					InterfaceChanged?.Invoke(this, new InterfaceArgs() { State = EnumInterfaceState.Removal, Interface = i });
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_start:
					ConnectionChanged?.Invoke(this, new ConnectionArgs() { State = EnumConnectionState.Start, Interface = i });
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_complete:
					ConnectionChanged?.Invoke(this, new ConnectionArgs() { State = EnumConnectionState.Complete, Interface = i });
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_attempt_fail:
					ConnectionChanged?.Invoke(this, new ConnectionArgs() { State = EnumConnectionState.Failed, Interface = i });
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_disconnecting:
					ConnectionChanged?.Invoke(this, new ConnectionArgs() { State = EnumConnectionState.Disconnecting, Interface = i });
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_disconnected:
					ConnectionChanged?.Invoke(this, new ConnectionArgs() { State = EnumConnectionState.Disconnected, Interface = i});
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_profile_change:
					ProfileChanged?.Invoke(this, new ProfileArgs() { State = EnumProfileState.Change, Interface = i });
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_profile_name_change:
					ProfileChanged?.Invoke(this, new ProfileArgs() { State = EnumProfileState.NameChange, Interface = i});
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_profile_unblocked:
					ProfileChanged?.Invoke(this, new ProfileArgs() { State = EnumProfileState.Unblocked, Interface = i });
					break;
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_profile_blocked:
					ProfileChanged?.Invoke(this, new ProfileArgs() { State = EnumProfileState.Blocked, Interface = i });
					break;
					// 2022/10/04 <<
			}
		}

		#region Dispose

		private bool _disposed = false;

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing">Whether to dispose managed resources</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_client.Dispose();
			}

			_disposed = true;
		}

		#endregion

		/// <summary>
		/// Enumerates wireless interface information.
		/// </summary>
		public IEnumerable<InterfaceInfo> EnumerateInterfaces() =>
			NativeWifi.EnumerateInterfaces(_client);

		/// <summary>
		/// Enumerates wireless interface and related connection information.
		/// </summary>
		public IEnumerable<InterfaceConnectionInfo> EnumerateInterfaceConnections() =>
			NativeWifi.EnumerateInterfaceConnections(_client);

		/// <summary>
		/// Asynchronously requests wireless interfaces to scan wireless LANs.
		/// </summary>
		public Task<IEnumerable<Guid>> ScanNetworksAsync(TimeSpan timeout, CancellationToken cancellationToken) =>
			NativeWifi.ScanNetworksAsync(_client, timeout, cancellationToken);

		/// <summary>
		/// Enumerates SSIDs of available wireless LANs.
		/// </summary>
		public IEnumerable<NetworkIdentifier> EnumerateAvailableNetworkSsids() =>
			NativeWifi.EnumerateAvailableNetworkSsids(_client);

		/// <summary>
		/// Enumerates SSIDs of connected wireless LANs.
		/// </summary>
		public IEnumerable<NetworkIdentifier> EnumerateConnectedNetworkSsids() =>
			NativeWifi.EnumerateConnectedNetworkSsids(_client);

		/// <summary>
		/// Enumerates wireless LAN information on available networks.
		/// </summary>
		public IEnumerable<AvailableNetworkPack> EnumerateAvailableNetworks() =>
			NativeWifi.EnumerateAvailableNetworks(_client);

		/// <summary>
		/// Enumerates wireless LAN information on available networks and group of associated BSS networks.
		/// </summary>
		public IEnumerable<AvailableNetworkGroupPack> EnumerateAvailableNetworkGroups() =>
			NativeWifi.EnumerateAvailableNetworkGroups(_client);

		/// <summary>
		/// Enumerates wireless LAN information on BSS networks.
		/// </summary>
		public IEnumerable<BssNetworkPack> EnumerateBssNetworks() =>
			NativeWifi.EnumerateBssNetworks(_client);

		/// <summary>
		/// Enumerates wireless profile names in preference order.
		/// </summary>
		public IEnumerable<string> EnumerateProfileNames() =>
			NativeWifi.EnumerateProfileNames(_client);

		/// <summary>
		/// Enumerates wireless profile information in preference order.
		/// </summary>
		public IEnumerable<ProfilePack> EnumerateProfiles() =>
			NativeWifi.EnumerateProfiles(_client);

		/// <summary>
		/// Enumerates wireless profile and related radio information in preference order.
		/// </summary>
		public IEnumerable<ProfileRadioPack> EnumerateProfileRadios() =>
			NativeWifi.EnumerateProfileRadios(_client);

		/// <summary>
		/// Sets (adds or overwrites) the content of a specified wireless profile.
		/// </summary>
		public bool SetProfile(Guid interfaceId, ProfileType profileType, string profileXml, string profileSecurity, bool overwrite) =>
			NativeWifi.SetProfile(_client, interfaceId, profileType, profileXml, profileSecurity, overwrite);

		/// <summary>
		/// Sets the position of a specified wireless profile in preference order.
		/// </summary>
		public bool SetProfilePosition(Guid interfaceId, string profileName, int position) =>
			NativeWifi.SetProfilePosition(_client, interfaceId, profileName, position);

		/// <summary>
		/// Sets (add or overwirte) the user data (credentials) for a specified wireless profile.
		/// </summary>
		public bool SetProfileEapXmlUserData(Guid interfaceId, string profileName, EapXmlType eapXmlType, string userDataXml) =>
			NativeWifi.SetProfileEapXmlUserData(_client, interfaceId, profileName, eapXmlType, userDataXml);

		/// <summary>
		/// Renames a specified wireless profile.
		/// </summary>
		public bool RenameProfile(Guid interfaceId, string oldProfileName, string newProfileName) =>
			NativeWifi.RenameProfile(_client, interfaceId, oldProfileName, newProfileName);

		/// <summary>
		/// Deletes a specified wireless profile.
		/// </summary>
		public bool DeleteProfile(Guid interfaceId, string profileName) =>
			NativeWifi.DeleteProfile(_client, interfaceId, profileName);

		/// <summary>
		/// Attempts to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		public bool ConnectNetwork(Guid interfaceId, string profileName, BssType bssType) =>
			NativeWifi.ConnectNetwork(_client, interfaceId, profileName, bssType);

		/// <summary>
		/// Asynchronously attempts to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		public Task<bool> ConnectNetworkAsync(Guid interfaceId, string profileName, BssType bssType, TimeSpan timeout, CancellationToken cancellationToken) =>
			NativeWifi.ConnectNetworkAsync(_client, interfaceId, profileName, bssType, null, timeout, cancellationToken);

		/// <summary>
		/// Disconnects from the wireless LAN associated to a specified wireless interface.
		/// </summary>
		public bool DisconnectNetwork(Guid interfaceId) =>
			NativeWifi.DisconnectNetwork(_client, interfaceId);

		/// <summary>
		/// Asynchronously disconnects from the wireless LAN associated to a specified wireless interface.
		/// </summary>
		public Task<bool> DisconnectNetworkAsync(Guid interfaceId, TimeSpan timeout, CancellationToken cancellationToken) =>
			NativeWifi.DisconnectNetworkAsync(_client, interfaceId, timeout, cancellationToken);
	}
}