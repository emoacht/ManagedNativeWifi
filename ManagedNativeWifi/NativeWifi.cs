using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ManagedNativeWifi.Win32;
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
		/// Enumerate GUIDs of wireless interfaces.
		/// </summary>
		/// <returns>GUIDs of wireless interfaces</returns>
		public static IEnumerable<Guid> EnumerateInterfaceGuids()
		{
			using (var client = new Base.WlanClient())
			{
				return Base.GetInterfaceInfoList(client.Handle)
					.Select(x => x.InterfaceGuid);
			}
		}

		#endregion

		#region Scan networks

		/// <summary>
		/// Asynchronously request wireless interfaces to scan (rescan) wireless LANs.
		/// </summary>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <returns>Interface GUIDs that the requests succeeded</returns>
		public static async Task<IEnumerable<Guid>> ScanNetworksAsync(TimeSpan timeoutDuration)
		{
			return await ScanNetworksAsync(timeoutDuration, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously request wireless interfaces to scan (rescan) wireless LANs.
		/// </summary>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Interface GUIDs that the requests succeeded</returns>
		public static async Task<IEnumerable<Guid>> ScanNetworksAsync(TimeSpan timeoutDuration, CancellationToken cancellationToken)
		{
			using (var client = new Base.WlanClient())
			{
				var interfaceInfoList = Base.GetInterfaceInfoList(client.Handle);
				var interfaceGuids = interfaceInfoList.Select(x => x.InterfaceGuid).ToArray();

				var tcs = new TaskCompletionSource<bool>();
				var handler = new ScanHandler(tcs, interfaceGuids);

				Action<IntPtr, IntPtr> callback = (data, context) =>
				{
					var notificationData = Marshal.PtrToStructure<WLAN_NOTIFICATION_DATA>(data);
					if (notificationData.NotificationSource != WLAN_NOTIFICATION_SOURCE_ACM)
						return;

					Debug.WriteLine("Callback: {0}", (WLAN_NOTIFICATION_ACM)notificationData.NotificationCode);

					switch (notificationData.NotificationCode)
					{
						case (uint)WLAN_NOTIFICATION_ACM.wlan_notification_acm_scan_complete:
							Debug.WriteLine("Scan succeeded.");
							handler.SetSuccess(notificationData.InterfaceGuid);
							break;
						case (uint)WLAN_NOTIFICATION_ACM.wlan_notification_acm_scan_fail:
							Debug.WriteLine("Scan failed.");
							handler.SetFailure(notificationData.InterfaceGuid);
							break;
					}
				};

				Base.RegisterNotification(client.Handle, WLAN_NOTIFICATION_SOURCE_ACM, callback);

				foreach (var interfaceGuid in interfaceGuids)
				{
					var result = Base.Scan(client.Handle, interfaceGuid);
					if (!result)
						handler.SetFailure(interfaceGuid);
				}

				var scanTask = tcs.Task;
				await Task.WhenAny(scanTask, Task.Delay(timeoutDuration, cancellationToken));

				return handler.Results;
			}
		}

		private class ScanHandler
		{
			private TaskCompletionSource<bool> _tcs;
			private readonly List<Guid> _targets = new List<Guid>();
			private readonly List<Guid> _results = new List<Guid>();

			public IEnumerable<Guid> Results => _results.ToArray();

			public ScanHandler(TaskCompletionSource<bool> tcs, IEnumerable<Guid> targets)
			{
				this._tcs = tcs;
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
				if ((_targets.Count <= 0) && !_tcs.Task.IsCompleted)
					Task.Run(() => _tcs.SetResult(true));
			}
		}

		#endregion

		#region Enumerate networks

		/// <summary>
		/// Enumerate SSIDs of available wireless LANs.
		/// </summary>
		/// <returns>SSIDs</returns>
		public static IEnumerable<NetworkIdentifier> EnumerateAvailableNetworkSsids()
		{
			using (var client = new Base.WlanClient())
			{
				var interfaceInfoList = Base.GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var availableNetworkList = Base.GetAvailableNetworkList(client.Handle, interfaceInfo.InterfaceGuid);

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
		/// Enumerate SSIDs of connected wireless LANs.
		/// </summary>
		/// <returns>SSIDs</returns>
		public static IEnumerable<NetworkIdentifier> EnumerateConnectedNetworkSsids()
		{
			using (var client = new Base.WlanClient())
			{
				var interfaceInfoList = Base.GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var connection = Base.GetConnectionAttributes(client.Handle, interfaceInfo.InterfaceGuid);
					if (connection.isState != WLAN_INTERFACE_STATE.wlan_interface_state_connected)
						continue;

					var association = connection.wlanAssociationAttributes;

					//Debug.WriteLine("Interface: {0}, SSID: {1}, BSSID {2}, Signal: {3}",
					//	interfaceInfo.strInterfaceDescription,
					//	association.dot11Ssid,
					//	association.dot11Bssid,
					//	association.wlanSignalQuality);

					yield return new NetworkIdentifier(association.dot11Ssid.ToBytes(), association.dot11Ssid.ToString());
				}
			}
		}

		/// <summary>
		/// Enumerate wireless LAN information on available networks.
		/// </summary>
		/// <returns>Wireless LAN information</returns>
		/// <remarks>If multiple profiles are associated with a same network, there will be multiple entries
		/// with the same SSID.</remarks>
		public static IEnumerable<AvailableNetworkPack> EnumerateAvailableNetworks()
		{
			using (var client = new Base.WlanClient())
			{
				var interfaceInfoList = Base.GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var availableNetworkList = Base.GetAvailableNetworkList(client.Handle, interfaceInfo.InterfaceGuid);

					foreach (var availableNetwork in availableNetworkList)
					{
						//Debug.WriteLine("Interface: {0}, SSID: {1}, Signal: {2}",
						//	interfaceInfo.strInterfaceDescription,
						//	availableNetwork.dot11Ssid,
						//	availableNetwork.wlanSignalQuality);

						yield return new AvailableNetworkPack(
							interfaceGuid: interfaceInfo.InterfaceGuid,
							interfaceDescription: interfaceInfo.strInterfaceDescription,
							ssid: new NetworkIdentifier(availableNetwork.dot11Ssid.ToBytes(), availableNetwork.dot11Ssid.ToString()),
							bssType: ConvertToBssType(availableNetwork.dot11BssType),
							signalQuality: (int)availableNetwork.wlanSignalQuality,
							profileName: availableNetwork.strProfileName);
					}
				}
			}
		}

		/// <summary>
		/// Enumerate wireless LAN information on BSS networks.
		/// </summary>
		/// <returns>Wireless LAN information</returns>
		public static IEnumerable<BssNetworkPack> EnumerateBssNetworks()
		{
			using (var client = new Base.WlanClient())
			{
				var interfaceInfoList = Base.GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var networkBssEntryList = Base.GetNetworkBssEntryList(client.Handle, interfaceInfo.InterfaceGuid);

					foreach (var networkBssEntry in networkBssEntryList)
					{
						//Debug.WriteLine("Interface: {0}, SSID: {1} BSSID: {2} Link: {3}",
						//	interfaceInfo.strInterfaceDescription,
						//	networkBssEntry.dot11Ssid,
						//	networkBssEntry.dot11Bssid,
						//	networkBssEntry.uLinkQuality);

						yield return new BssNetworkPack(
							interfaceGuid: interfaceInfo.InterfaceGuid,
							interfaceDescription: interfaceInfo.strInterfaceDescription,
							ssid: new NetworkIdentifier(networkBssEntry.dot11Ssid.ToBytes(), networkBssEntry.dot11Ssid.ToString()),
							bssType: ConvertToBssType(networkBssEntry.dot11BssType),
							bssid: new NetworkIdentifier(networkBssEntry.dot11Bssid.ToBytes(), networkBssEntry.dot11Bssid.ToString()),
							linkQuality: (int)networkBssEntry.uLinkQuality);
					}
				}
			}
		}

		#endregion

		#region Enumerate profiles

		/// <summary>
		/// Enumerate wireless profile names in preference order.
		/// </summary>
		/// <returns>Wireless profile names</returns>
		public static IEnumerable<string> EnumerateProfileNames()
		{
			using (var client = new Base.WlanClient())
			{
				var interfaceInfoList = Base.GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var profileInfoList = Base.GetProfileInfoList(client.Handle, interfaceInfo.InterfaceGuid);

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
		/// Enumerate wireless profile information in preference order.
		/// </summary>
		/// <returns>Wireless profile information</returns>
		public static IEnumerable<ProfilePack> EnumerateProfiles()
		{
			using (var client = new Base.WlanClient())
			{
				var interfaceInfoList = Base.GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var availableNetworkList = Base.GetAvailableNetworkList(client.Handle, interfaceInfo.InterfaceGuid)
						.Where(x => !string.IsNullOrWhiteSpace(x.strProfileName))
						.ToArray();

					var connection = Base.GetConnectionAttributes(client.Handle, interfaceInfo.InterfaceGuid);
					var interfaceIsConnected = (connection.isState == WLAN_INTERFACE_STATE.wlan_interface_state_connected);

					var profileInfoList = Base.GetProfileInfoList(client.Handle, interfaceInfo.InterfaceGuid);

					int position = 0;

					foreach (var profileInfo in profileInfoList)
					{
						var availableNetwork = availableNetworkList.FirstOrDefault(x => x.strProfileName.Equals(profileInfo.strProfileName, StringComparison.Ordinal));
						var signalQuality = (int)availableNetwork.wlanSignalQuality;

						var profileIsConnected = interfaceIsConnected && profileInfo.strProfileName.Equals(connection.strProfileName, StringComparison.Ordinal);

						//Debug.WriteLine("Interface: {0}, Profile: {1}, Signal {2}, Position: {3}, Connected {4}",
						//	interfaceInfo.strInterfaceDescription,
						//	profileInfo.strProfileName,
						//	signalQuality,
						//	position,
						//	profileIsConnected);

						var profile = GetProfile(
							client.Handle,
							interfaceInfo.InterfaceGuid,
							profileInfo.strProfileName,
							interfaceInfo.strInterfaceDescription,
							signalQuality,
							position++,
							profileIsConnected);

						if (profile != null)
							yield return profile;
					}
				}
			}
		}

		/// <summary>
		/// Get a specified wireless profile information.
		/// </summary>
		/// <param name="clientHandle">Client handle</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceDescription">Interface description</param>
		/// <param name="signalQuality">Signal quality</param>
		/// <param name="position">Position in preference order</param> 
		/// <param name="isConnected">Whether this profile is connected to a wireless LAN</param>
		/// <returns>Wireless profile information</returns>
		/// <remarks>
		/// For profile elements, see
		/// https://msdn.microsoft.com/en-us/library/windows/desktop/ms707381.aspx 
		/// </remarks>
		private static ProfilePack GetProfile(SafeClientHandle clientHandle, Guid interfaceGuid, string profileName, string interfaceDescription, int signalQuality, int position, bool isConnected)
		{
			ProfileType profileType;
			var source = Base.GetProfile(clientHandle, interfaceGuid, profileName, out profileType);
			if (string.IsNullOrWhiteSpace(source))
				return null;

			XElement rootXml;
			using (var sr = new StringReader(source))
				rootXml = XElement.Load(sr);

			var ns = rootXml.Name.Namespace;

			var ssidXml = rootXml.Descendants(ns + "SSID").FirstOrDefault();
			var ssidHexadecimalString = ssidXml?.Descendants(ns + "hex").FirstOrDefault()?.Value;
			var ssidBytes = ConvertFromHexadecimalStringToBytes(ssidHexadecimalString);
			var ssidString = ssidXml?.Descendants(ns + "name").FirstOrDefault()?.Value;

			var connectionTypeXml = rootXml.Descendants(ns + "connectionType").FirstOrDefault();
			var bssType = ConvertToBssType(connectionTypeXml?.Value);

			var connectionModeXml = rootXml.Descendants(ns + "connectionMode").FirstOrDefault();
			var isAutomatic = (connectionModeXml?.Value.Equals("auto", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();

			var authenticationXml = rootXml.Descendants(ns + "authentication").FirstOrDefault();
			var authentication = authenticationXml?.Value;

			var encryptionXml = rootXml.Descendants(ns + "encryption").FirstOrDefault();
			var encryption = encryptionXml?.Value;

			//Debug.WriteLine("SSID: {0}, BssType: {1}, Authentication: {2}, Encryption: {3}, Automatic: {4}",
			//	ssidString,
			//	bssType,
			//	authentication,
			//	encryption,
			//	isAutomatic);

			return new ProfilePack(
				name: profileName,
				interfaceGuid: interfaceGuid,
				interfaceDescription: interfaceDescription,
				profileType: profileType,
				profileXml: source,
				ssid: new NetworkIdentifier(ssidBytes, ssidString),
				bssType: bssType,
				authentication: authentication,
				encryption: encryption,
				signalQuality: signalQuality,
				position: position,
				isAutomatic: isAutomatic,
				isConnected: isConnected);
		}

		#endregion

		#region Set profile

		/// <summary>
		/// Set (add or overwrite) the content of a specific profile.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
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
		public static bool SetProfile(Guid interfaceGuid, ProfileType profileType, string profileXml, string profileSecurity, bool overwrite)
		{
			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			if (string.IsNullOrWhiteSpace(profileXml))
				throw new ArgumentNullException(nameof(profileXml));

			using (var client = new Base.WlanClient())
			{
				return Base.SetProfile(client.Handle, interfaceGuid, profileType, profileXml, profileSecurity, overwrite);
			}
		}

		#endregion

		#region Set profile position

		/// <summary>
		/// Set the position of a specified wireless profile in preference order.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="profileName">Profile name</param>
		/// <param name="position">Position (starting from 0)</param>
		/// <returns>True if successfully set.</returns>
		public static bool SetProfilePosition(Guid interfaceGuid, string profileName, int position)
		{
			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			if (position < 0)
				throw new ArgumentOutOfRangeException(nameof(position));

			using (var client = new Base.WlanClient())
			{
				return Base.SetProfilePosition(client.Handle, interfaceGuid, profileName, (uint)position);
			}
		}

		#endregion

		#region Delete profile

		/// <summary>
		/// Delete a specified wireless profile.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="profileName">Profile name</param>
		/// <returns>True if successfully deleted. False if could not delete.</returns>
		public static bool DeleteProfile(Guid interfaceGuid, string profileName)
		{
			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			using (var client = new Base.WlanClient())
			{
				return Base.DeleteProfile(client.Handle, interfaceGuid, profileName);
			}
		}

		#endregion

		#region Connect/Disconnect

		/// <summary>
		/// Attempt to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="profileName">Profile name</param>
		/// <param name="bssType">BSS type</param>
		/// <returns>True if successfully requested the connection. False if failed.</returns>
		public static bool ConnectNetwork(Guid interfaceGuid, string profileName, BssType bssType = BssType.Any)
		{
			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			using (var client = new Base.WlanClient())
			{
				return Base.Connect(client.Handle, interfaceGuid, profileName, ConvertFromBssType(bssType));
			}
		}

		/// <summary>
		/// Asynchronously attempt to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="profileName">Profile name</param>
		/// <param name="bssType">BSS type</param>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <returns>True if successfully connected. False if failed or timed out.</returns>
		public static async Task<bool> ConnectNetworkAsync(Guid interfaceGuid, string profileName, BssType bssType, TimeSpan timeoutDuration)
		{
			return await ConnectNetworkAsync(interfaceGuid, profileName, bssType, timeoutDuration, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously attempt to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="profileName">Profile name</param> 
		/// <param name="bssType">BSS type</param>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>True if successfully connected. False if failed or timed out.</returns>
		public static async Task<bool> ConnectNetworkAsync(Guid interfaceGuid, string profileName, BssType bssType, TimeSpan timeoutDuration, CancellationToken cancellationToken)
		{
			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			if (timeoutDuration < TimeSpan.Zero)
				throw new ArgumentException(nameof(timeoutDuration));

			using (var client = new Base.WlanClient())
			{
				var tcs = new TaskCompletionSource<bool>();

				Action<IntPtr, IntPtr> callback = (data, context) =>
				{
					var notificationData = Marshal.PtrToStructure<WLAN_NOTIFICATION_DATA>(data);
					if (notificationData.NotificationSource != WLAN_NOTIFICATION_SOURCE_ACM)
						return;

					Debug.WriteLine("Callback: {0}", (WLAN_NOTIFICATION_ACM)notificationData.NotificationCode);

					switch (notificationData.NotificationCode)
					{
						case (uint)WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_complete:
							Task.Run(() => tcs.SetResult(true));
							break;
						case (uint)WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_attempt_fail:
							Task.Run(() => tcs.SetResult(false));
							break;
					}
				};

				Base.RegisterNotification(client.Handle, WLAN_NOTIFICATION_SOURCE_ACM, callback);

				var result = Base.Connect(client.Handle, interfaceGuid, profileName, ConvertFromBssType(bssType));
				if (!result)
					tcs.SetResult(false);

				var connectTask = tcs.Task;
				var completedTask = await Task.WhenAny(connectTask, Task.Delay(timeoutDuration, cancellationToken));

				return (completedTask == connectTask) && connectTask.Result;
			}
		}

		/// <summary>
		/// Disconnect from the wireless LAN associated to a specified wireless interface.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <returns>True if successfully requested the disconnection. False if failed.</returns>
		public static bool DisconnectNetwork(Guid interfaceGuid)
		{
			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			using (var client = new Base.WlanClient())
			{
				return Base.Disconnect(client.Handle, interfaceGuid);
			}
		}

		/// <summary>
		/// Asynchronously disconnect from the wireless LAN associated to a specified wireless interface.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <returns>True if successfully disconnected. False if failed or timed out.</returns>
		public static async Task<bool> DisconnectNetworkAsync(Guid interfaceGuid, TimeSpan timeoutDuration)
		{
			return await DisconnectNetworkAsync(interfaceGuid, timeoutDuration, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously disconnect from the wireless LAN associated to a specified wireless interface.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>True if successfully disconnected. False if failed or timed out.</returns>
		public static async Task<bool> DisconnectNetworkAsync(Guid interfaceGuid, TimeSpan timeoutDuration, CancellationToken cancellationToken)
		{
			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			if (timeoutDuration < TimeSpan.Zero)
				throw new ArgumentException(nameof(timeoutDuration));

			using (var client = new Base.WlanClient())
			{
				var tcs = new TaskCompletionSource<bool>();

				Action<IntPtr, IntPtr> callback = (data, context) =>
				{
					var notificationData = Marshal.PtrToStructure<WLAN_NOTIFICATION_DATA>(data);
					if (notificationData.NotificationSource != WLAN_NOTIFICATION_SOURCE_ACM)
						return;

					Debug.WriteLine("Callback: {0}", (WLAN_NOTIFICATION_ACM)notificationData.NotificationCode);

					switch (notificationData.NotificationCode)
					{
						case (uint)WLAN_NOTIFICATION_ACM.wlan_notification_acm_disconnected:
							Task.Run(() => tcs.SetResult(true));
							break;
					}
				};

				Base.RegisterNotification(client.Handle, WLAN_NOTIFICATION_SOURCE_ACM, callback);

				var result = Base.Disconnect(client.Handle, interfaceGuid);
				if (!result)
					tcs.SetResult(false);

				var disconnectTask = tcs.Task;
				var completedTask = await Task.WhenAny(disconnectTask, Task.Delay(timeoutDuration, cancellationToken));

				return (completedTask == disconnectTask) && disconnectTask.Result;
			}
		}

		#endregion

		#region Helper

		private static DOT11_BSS_TYPE ConvertFromBssType(BssType source)
		{
			switch (source)
			{
				case BssType.Infrastructure:
					return DOT11_BSS_TYPE.dot11_BSS_type_infrastructure;
				case BssType.Independent:
					return DOT11_BSS_TYPE.dot11_BSS_type_independent;
				default:
					return DOT11_BSS_TYPE.dot11_BSS_type_any;
			}
		}

		private static BssType ConvertToBssType(DOT11_BSS_TYPE source)
		{
			switch (source)
			{
				case DOT11_BSS_TYPE.dot11_BSS_type_infrastructure:
					return BssType.Infrastructure;
				case DOT11_BSS_TYPE.dot11_BSS_type_independent:
					return BssType.Independent;
				default:
					return BssType.Any;
			}
		}

		private static BssType ConvertToBssType(string source)
		{
			if (string.IsNullOrWhiteSpace(source))
			{
				return default(BssType);
			}
			if (source.Equals("ESS", StringComparison.OrdinalIgnoreCase))
			{
				return BssType.Infrastructure;
			}
			if (source.Equals("IBSS", StringComparison.OrdinalIgnoreCase))
			{
				return BssType.Independent;
			}
			return BssType.Any;
		}

		private static byte[] ConvertFromHexadecimalStringToBytes(string source)
		{
			if (string.IsNullOrWhiteSpace(source))
				return null;

			var buff = new byte[source.Length / 2];

			for (int i = 0; i < buff.Length; i++)
			{
				try
				{
					buff[i] = Convert.ToByte(source.Substring(i * 2, 2), 16);
				}
				catch (FormatException)
				{
					break;
				}
			}
			return buff;
		}

		#endregion
	}
}