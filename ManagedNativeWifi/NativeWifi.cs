using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace ManagedNativeWifi
{
	/// <summary>
	/// A managed implementation of Native Wifi API
	/// </summary>
	public class NativeWifi
	{
		#region Scan networks

		/// <summary>
		/// Asynchronously request wireless interfaces to scan (rescan) wireless LANs.
		/// </summary>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <returns>Interface GUIDs that the requests succeeded</returns>
		public static async Task<IEnumerable<Guid>> ScanAsync(TimeSpan timeoutDuration)
		{
			return await ScanAsync(timeoutDuration, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously request wireless interfaces to scan (rescan) wireless LANs.
		/// </summary>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Interface GUIDs that the requests succeeded</returns>
		public static async Task<IEnumerable<Guid>> ScanAsync(TimeSpan timeoutDuration, CancellationToken cancellationToken)
		{
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);
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

				RegisterNotification(client.Handle, WLAN_NOTIFICATION_SOURCE_ACM, callback);

				foreach (var interfaceGuid in interfaceGuids)
				{
					var result = Scan(client.Handle, interfaceGuid);
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
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var availableNetworkList = GetAvailableNetworkList(client.Handle, interfaceInfo.InterfaceGuid);

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
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var connection = GetConnectionAttributes(client.Handle, interfaceInfo.InterfaceGuid);
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
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var availableNetworkList = GetAvailableNetworkList(client.Handle, interfaceInfo.InterfaceGuid);

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
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var networkBssEntryList = GetNetworkBssEntryList(client.Handle, interfaceInfo.InterfaceGuid);

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
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var profileInfoList = GetProfileInfoList(client.Handle, interfaceInfo.InterfaceGuid);

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
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var availableNetworkList = GetAvailableNetworkList(client.Handle, interfaceInfo.InterfaceGuid)
						.Where(x => !string.IsNullOrWhiteSpace(x.strProfileName))
						.ToArray();

					var connection = GetConnectionAttributes(client.Handle, interfaceInfo.InterfaceGuid);
					var interfaceIsConnected = (connection.isState == WLAN_INTERFACE_STATE.wlan_interface_state_connected);

					var profileInfoList = GetProfileInfoList(client.Handle, interfaceInfo.InterfaceGuid);

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
							profileInfo.strProfileName,
							interfaceInfo.InterfaceGuid,
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
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="interfaceDescription">Interface description</param>
		/// <param name="signalQuality">Signal quality</param>
		/// <param name="position">Position in preference order</param> 
		/// <param name="isConnected">Whether this profile is connected to a wireless LAN</param>
		/// <returns>Wireless profile information</returns>
		/// <remarks>
		/// For profile elements, see
		/// https://msdn.microsoft.com/en-us/library/windows/desktop/ms707381.aspx 
		/// </remarks>
		private static ProfilePack GetProfile(SafeClientHandle clientHandle, string profileName, Guid interfaceGuid, string interfaceDescription, int signalQuality, int position, bool isConnected)
		{
			var source = GetProfileXml(clientHandle, interfaceGuid, profileName);
			if (string.IsNullOrWhiteSpace(source))
				return null;

			XElement rootXml;
			using (var sr = new StringReader(source))
				rootXml = XElement.Load(sr);

			var ns = rootXml.Name.Namespace;

			var ssidXml = rootXml.Descendants(ns + "SSID").FirstOrDefault();
			var ssidHexadecimalString = ssidXml?.Descendants(ns + "hex").FirstOrDefault()?.Value;
			var ssidBytes = ConvertFromHexadecimalString(ssidHexadecimalString);
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
				ssid: new NetworkIdentifier(ssidBytes, ssidString),
				bssType: bssType,
				authentication: authentication,
				encryption: encryption,
				signalQuality: signalQuality,
				position: position,
				isAutomatic: isAutomatic,
				isConnected: isConnected,
				xml: source);
		}

		#endregion

		#region Set profile position

		/// <summary>
		/// Set the position of a specified wireless profile in preference order.
		/// </summary>
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="position">Position (starting from 0)</param>
		/// <returns>True if set.</returns>
		public static bool SetProfilePosition(string profileName, Guid interfaceGuid, int position)
		{
			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			if (position < 0)
				throw new ArgumentOutOfRangeException(nameof(position));

			using (var client = new WlanClient())
			{
				return SetProfilePosition(client.Handle, interfaceGuid, profileName, (uint)position);
			}
		}

		#endregion

		#region Delete profile

		/// <summary>
		/// Delete a specified wireless profile.
		/// </summary>
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <returns>True if deleted. False if could not delete.</returns>
		public static bool DeleteProfile(string profileName, Guid interfaceGuid)
		{
			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			using (var client = new WlanClient())
			{
				return DeleteProfile(client.Handle, interfaceGuid, profileName);
			}
		}

		#endregion

		#region Connect/Disconnect

		/// <summary>
		/// Attempt to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="bssType">BSS type</param>
		/// <returns>True if successfully requested the connection. False if failed.</returns>
		public static bool Connect(string profileName, Guid interfaceGuid, BssType bssType = BssType.Any)
		{
			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			using (var client = new WlanClient())
			{
				return Connect(client.Handle, interfaceGuid, profileName, ConvertFromBssType(bssType));
			}
		}

		/// <summary>
		/// Asynchronously attempt to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="bssType">BSS type</param>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <returns>True if successfully connected. False if failed or timed out.</returns>
		public static async Task<bool> ConnectAsync(string profileName, Guid interfaceGuid, BssType bssType, TimeSpan timeoutDuration)
		{
			return await ConnectAsync(profileName, interfaceGuid, bssType, timeoutDuration, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously attempt to connect to the wireless LAN associated to a specified wireless profile.
		/// </summary>
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="bssType">BSS type</param>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>True if successfully connected. False if failed or timed out.</returns>
		public static async Task<bool> ConnectAsync(string profileName, Guid interfaceGuid, BssType bssType, TimeSpan timeoutDuration, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			if (timeoutDuration < TimeSpan.Zero)
				throw new ArgumentException(nameof(timeoutDuration));

			using (var client = new WlanClient())
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

				RegisterNotification(client.Handle, WLAN_NOTIFICATION_SOURCE_ACM, callback);

				var result = Connect(client.Handle, interfaceGuid, profileName, ConvertFromBssType(bssType));
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
		public static bool Disconnect(Guid interfaceGuid)
		{
			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			using (var client = new WlanClient())
			{
				return Disconnect(client.Handle, interfaceGuid);
			}
		}

		/// <summary>
		/// Asynchronously disconnect from the wireless LAN associated to a specified wireless interface.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <returns>True if successfully disconnected. False if failed or timed out.</returns>
		public static async Task<bool> DisconnectAsync(Guid interfaceGuid, TimeSpan timeoutDuration)
		{
			return await DisconnectAsync(interfaceGuid, timeoutDuration, CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously disconnect from the wireless LAN associated to a specified wireless interface.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>True if successfully disconnected. False if failed or timed out.</returns>
		public static async Task<bool> DisconnectAsync(Guid interfaceGuid, TimeSpan timeoutDuration, CancellationToken cancellationToken)
		{
			if (interfaceGuid == default(Guid))
				throw new ArgumentException(nameof(interfaceGuid));

			if (timeoutDuration < TimeSpan.Zero)
				throw new ArgumentException(nameof(timeoutDuration));

			using (var client = new WlanClient())
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

				RegisterNotification(client.Handle, WLAN_NOTIFICATION_SOURCE_ACM, callback);

				var result = Disconnect(client.Handle, interfaceGuid);
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

		private static byte[] ConvertFromHexadecimalString(string source)
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

		#region Base

		private class WlanClient : IDisposable
		{
			private SafeClientHandle _clientHandle = null;

			public SafeClientHandle Handle => _clientHandle;

			public WlanClient()
			{
				uint negotiatedVersion;
				var result = WlanOpenHandle(
					2, // Client version for Windows Vista and Windows Server 2008
					IntPtr.Zero,
					out negotiatedVersion,
					out _clientHandle);

				CheckResult(result, nameof(WlanOpenHandle), true);
			}

			#region Dispose

			private bool _disposed = false;

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				_clientHandle?.Dispose();

				_disposed = true;
			}

			#endregion
		}

		private static IEnumerable<WLAN_INTERFACE_INFO> GetInterfaceInfoList(SafeClientHandle clientHandle)
		{
			var interfaceList = IntPtr.Zero;
			try
			{
				var result = WlanEnumInterfaces(
					clientHandle,
					IntPtr.Zero,
					out interfaceList);

				return CheckResult(result, nameof(WlanEnumInterfaces), true)
					? new WLAN_INTERFACE_INFO_LIST(interfaceList).InterfaceInfo
					: null; // Not to be used
			}
			finally
			{
				if (interfaceList != IntPtr.Zero)
					WlanFreeMemory(interfaceList);
			}
		}

		private static bool Scan(SafeClientHandle clientHandle, Guid interfaceGuid)
		{
			var result = WlanScan(
				clientHandle,
				interfaceGuid,
				IntPtr.Zero,
				IntPtr.Zero,
				IntPtr.Zero);

			// ERROR_NDIS_DOT11_POWER_STATE_INVALID will be returned if the interface is turned off.
			return CheckResult(result, nameof(WlanScan), false);
		}

		private static IEnumerable<WLAN_AVAILABLE_NETWORK> GetAvailableNetworkList(SafeClientHandle clientHandle, Guid interfaceGuid)
		{
			var availableNetworkList = IntPtr.Zero;
			try
			{
				var result = WlanGetAvailableNetworkList(
					clientHandle,
					interfaceGuid,
					WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_MANUAL_HIDDEN_PROFILES,
					IntPtr.Zero,
					out availableNetworkList);

				// ERROR_NDIS_DOT11_POWER_STATE_INVALID will be returned if the interface is turned off.
				return CheckResult(result, nameof(WlanGetAvailableNetworkList), false)
					? new WLAN_AVAILABLE_NETWORK_LIST(availableNetworkList).Network
					: new WLAN_AVAILABLE_NETWORK[0];
			}
			finally
			{
				if (availableNetworkList != IntPtr.Zero)
					WlanFreeMemory(availableNetworkList);
			}
		}

		private static IEnumerable<WLAN_BSS_ENTRY> GetNetworkBssEntryList(SafeClientHandle clientHandle, Guid interfaceGuid)
		{
			var wlanBssList = IntPtr.Zero;
			try
			{
				var result = WlanGetNetworkBssList(
					clientHandle,
					interfaceGuid,
					IntPtr.Zero,
					DOT11_BSS_TYPE.dot11_BSS_type_any,
					false,
					IntPtr.Zero,
					out wlanBssList);

				// ERROR_NDIS_DOT11_POWER_STATE_INVALID will be returned if the interface is turned off.
				return CheckResult(result, nameof(WlanGetNetworkBssList), false)
					? new WLAN_BSS_LIST(wlanBssList).wlanBssEntries
					: new WLAN_BSS_ENTRY[0];
			}
			finally
			{
				if (wlanBssList != IntPtr.Zero)
					WlanFreeMemory(wlanBssList);
			}
		}

		private static WLAN_CONNECTION_ATTRIBUTES GetConnectionAttributes(SafeClientHandle clientHandle, Guid interfaceGuid)
		{
			var queryData = IntPtr.Zero;
			try
			{
				uint dataSize;
				var result = WlanQueryInterface(
					clientHandle,
					interfaceGuid,
					WLAN_INTF_OPCODE.wlan_intf_opcode_current_connection,
					IntPtr.Zero,
					out dataSize,
					ref queryData,
					IntPtr.Zero);

				// ERROR_INVALID_STATE will be returned if the client is not connected to a network.
				return CheckResult(result, nameof(WlanQueryInterface), false)
					? Marshal.PtrToStructure<WLAN_CONNECTION_ATTRIBUTES>(queryData)
					: default(WLAN_CONNECTION_ATTRIBUTES);
			}
			finally
			{
				if (queryData != IntPtr.Zero)
					WlanFreeMemory(queryData);
			}
		}

		private static IEnumerable<WLAN_PROFILE_INFO> GetProfileInfoList(SafeClientHandle clientHandle, Guid interfaceGuid)
		{
			var profileList = IntPtr.Zero;
			try
			{
				var result = WlanGetProfileList(
					clientHandle,
					interfaceGuid,
					IntPtr.Zero,
					out profileList);

				return CheckResult(result, nameof(WlanGetProfileList), false)
					? new WLAN_PROFILE_INFO_LIST(profileList).ProfileInfo
					: new WLAN_PROFILE_INFO[0];
			}
			finally
			{
				if (profileList != IntPtr.Zero)
					WlanFreeMemory(profileList);
			}
		}

		private static string GetProfileXml(SafeClientHandle clientHandle, Guid interfaceGuid, string profileName)
		{
			var profileXml = IntPtr.Zero;
			try
			{
				uint flags = 0U;
				uint grantedAccess;
				var result = WlanGetProfile(
					clientHandle,
					interfaceGuid,
					profileName,
					IntPtr.Zero,
					out profileXml,
					ref flags,
					out grantedAccess);

				// ERROR_NOT_FOUND will be returned if the profile is not found.
				return CheckResult(result, nameof(WlanGetProfile), false)
					? Marshal.PtrToStringUni(profileXml)
					: null; // To be used
			}
			finally
			{
				if (profileXml != IntPtr.Zero)
					WlanFreeMemory(profileXml);
			}
		}

		private static bool SetProfilePosition(SafeClientHandle clientHandle, Guid interfaceGuid, string profileName, uint position)
		{
			var result = WlanSetProfilePosition(
				clientHandle,
				interfaceGuid,
				profileName,
				position,
				IntPtr.Zero);

			// ERROR_INVALID_PARAMETER will be returned if the interface is removed.
			// ERROR_NOT_FOUND will be returned if the position of a profile is invalid.
			return CheckResult(result, nameof(WlanSetProfilePosition), false);
		}

		private static bool DeleteProfile(SafeClientHandle clientHandle, Guid interfaceGuid, string profileName)
		{
			var result = WlanDeleteProfile(
				clientHandle,
				interfaceGuid,
				profileName,
				IntPtr.Zero);

			// ERROR_INVALID_PARAMETER will be returned if the interface is removed.
			// ERROR_NOT_FOUND will be returned if the profile is not found.
			return CheckResult(result, nameof(WlanDeleteProfile), false);
		}

		private static bool Connect(SafeClientHandle clientHandle, Guid interfaceGuid, string profileName, DOT11_BSS_TYPE bssType)
		{
			var connectionParameters = new WLAN_CONNECTION_PARAMETERS
			{
				wlanConnectionMode = WLAN_CONNECTION_MODE.wlan_connection_mode_profile,
				strProfile = profileName,
				dot11BssType = bssType,
				dwFlags = 0U
			};

			var result = WlanConnect(
				clientHandle,
				interfaceGuid,
				ref connectionParameters,
				IntPtr.Zero);

			// ERROR_NOT_FOUND will be returned if the interface is removed.
			return CheckResult(result, nameof(WlanConnect), false);
		}

		private static bool Disconnect(SafeClientHandle clientHandle, Guid interfaceGuid)
		{
			var result = WlanDisconnect(
				clientHandle,
				interfaceGuid,
				IntPtr.Zero);

			// ERROR_NOT_FOUND will be returned if the interface is removed.
			return CheckResult(result, nameof(WlanDisconnect), false);
		}

		private static void RegisterNotification(SafeClientHandle clientHandle, uint notificationSource, Action<IntPtr, IntPtr> callback)
		{
			// Storing a delegate in class field is necessary to prevent garbage collector from collecting it
			// before the delegate is called. Otherwise, CallbackOnCollectedDelegate may occur.
			_notificationCallback = new WLAN_NOTIFICATION_CALLBACK(callback);

			var result = WlanRegisterNotification(clientHandle,
				notificationSource,
				false,
				_notificationCallback,
				IntPtr.Zero,
				IntPtr.Zero,
				0);

			CheckResult(result, nameof(WlanRegisterNotification), true);
		}

		private static WLAN_NOTIFICATION_CALLBACK _notificationCallback;

		private static bool CheckResult(uint result, string methodName, bool willThrowOnFailure)
		{
			if (result == ERROR_SUCCESS)
				return true;

			switch (result)
			{
				case ERROR_INVALID_PARAMETER:
				case ERROR_INVALID_STATE:
				case ERROR_NOT_FOUND:
				case ERROR_ACCESS_DENIED:
				case ERROR_NOT_SUPPORTED:
				case ERROR_SERVICE_NOT_ACTIVE:
				case ERROR_NDIS_DOT11_AUTO_CONFIG_ENABLED:
				case ERROR_NDIS_DOT11_MEDIA_IN_USE:
				case ERROR_NDIS_DOT11_POWER_STATE_INVALID:
					if (!willThrowOnFailure)
						return false;
					else
						goto default;

				case ERROR_NOT_ENOUGH_MEMORY:
					throw new OutOfMemoryException(methodName);

				case ERROR_INVALID_HANDLE:
				default:
					throw CreateWin32Exception(result, methodName);
			}
		}

		private static Win32Exception CreateWin32Exception(uint errorCode, string methodName)
		{
			var sb = new StringBuilder(512); // This 512 capacity is arbitrary.

			var result = FormatMessage(
			  FORMAT_MESSAGE_FROM_SYSTEM,
			  IntPtr.Zero,
			  errorCode,
			  0x0409, // US (English)
			  sb,
			  sb.Capacity,
			  IntPtr.Zero);

			var message = $"Method: {methodName}, Code: {errorCode}";
			if (0 < result)
				message += $", Message: {sb}";

			return new Win32Exception((int)errorCode, message);
		}

		#endregion
	}
}