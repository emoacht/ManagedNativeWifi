using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
		#region Type

		/// <summary>
		/// BSS type
		/// </summary>
		public enum BssType
		{
			/// <summary>
			/// None
			/// </summary>
			None = 0,

			/// <summary>
			/// Infrastructure BSS network
			/// </summary>
			Infrastructure,

			/// <summary>
			/// Independent BSS (IBSS) network (Ad hoc network)
			/// </summary>
			Independent,

			/// <summary>
			/// Any BSS network
			/// </summary>
			Any
		}

		/// <summary>
		/// Wireless profile information
		/// </summary>
		public class ProfilePack
		{
			/// <summary>
			/// Profile name
			/// </summary>
			public string Name { get; private set; }

			/// <summary>
			/// GUID of associated wireless LAN interface
			/// </summary>
			public Guid InterfaceGuid { get; private set; }

			/// <summary>
			/// Description of associated wireless LAN interface
			/// </summary>
			public string InterfaceDescription { get; private set; }

			/// <summary>
			/// SSID of associated wireless LAN
			/// </summary>
			public string Ssid { get; private set; }

			/// <summary>
			/// BSS type of associated wireless LAN
			/// </summary>
			public BssType BssType { get; private set; }

			/// <summary>
			/// Authentication type of associated wireless LAN
			/// </summary>
			public string Authentication { get; private set; }

			/// <summary>
			/// Encryption type of associated wireless LAN
			/// </summary>
			public string Encryption { get; private set; }

			/// <summary>
			/// Position in preference order of associated wireless LAN interface
			/// </summary>
			public int Position { get; private set; }

			/// <summary>
			/// Whether this profile is set to be automatically connected
			/// </summary>
			public bool IsAutomatic { get; private set; }

			/// <summary>
			/// Signal level of associated wireless LAN
			/// </summary>
			public int Signal { get; private set; }

			/// <summary>
			/// Whether this profile is currently connected
			/// </summary>
			public bool IsConnected { get; private set; }

			public ProfilePack(
				string name,
				Guid interfaceGuid,
				string interfaceDescription,
				string ssid,
				BssType bssType,
				string authentication,
				string encryption,
				int position,
				bool isAutomatic,
				int signal,
				bool isConnected)
			{
				this.Name = name;
				this.InterfaceGuid = interfaceGuid;
				this.InterfaceDescription = interfaceDescription;
				this.Ssid = ssid;
				this.BssType = bssType;
				this.Authentication = authentication;
				this.Encryption = encryption;
				this.Position = position;
				this.IsAutomatic = isAutomatic;
				this.Signal = signal;
				this.IsConnected = isConnected;
			}
		}

		/// <summary>
		/// Wireless LAN information
		/// </summary>
		public class NetworkPack
		{
			/// <summary>
			/// GUID of associated wireless LAN interface
			/// </summary>
			public Guid InterfaceGuid { get; private set; }

			/// <summary>
			/// SSID
			/// </summary>
			public string Ssid { get; private set; }

			/// <summary>
			/// BSS type
			/// </summary>
			public BssType BssType { get; private set; }

			/// <summary>
			/// Signal level
			/// </summary>
			public int Signal { get; private set; }

			/// <summary>
			/// Name of associated wireless profile
			/// </summary>
			public string ProfileName { get; private set; }

			public NetworkPack(Guid interfaceGuid, string ssid, BssType bssType, int signal, string profileName)
			{
				this.InterfaceGuid = interfaceGuid;
				this.Ssid = ssid;
				this.BssType = bssType;
				this.Signal = signal;
				this.ProfileName = profileName;
			}
		}

		#endregion

		#region Scan networks

		/// <summary>
		/// Request wireless interfaces to scan available wireless LANs.
		/// </summary>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <returns>Interface GUIDs that the requests succeeded</returns>
		public static async Task<IEnumerable<Guid>> ScanAsync(TimeSpan timeoutDuration)
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
				await Task.WhenAny(scanTask, Task.Delay(timeoutDuration));

				return handler.Results;
			}
		}

		private class ScanHandler
		{
			private TaskCompletionSource<bool> _tcs;
			private readonly List<Guid> _targets = new List<Guid>();
			private readonly List<Guid> _results = new List<Guid>();

			public IEnumerable<Guid> Results { get { return _results.ToArray(); } }

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
		public static IEnumerable<string> EnumerateAvailableNetworkSsids()
		{
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var availableNetworkList = GetAvailableNetworkList(client.Handle, interfaceInfo.InterfaceGuid);

					foreach (var availableNetwork in availableNetworkList)
					{
						Debug.WriteLine("Interface: {0}, SSID: {1}",
							interfaceInfo.strInterfaceDescription,
							availableNetwork.dot11Ssid.ToSsidString());

						yield return availableNetwork.dot11Ssid.ToSsidString();
					}
				}
			}
		}

		/// <summary>
		/// Enumerate available wireless LANs.
		/// </summary>
		/// <returns>Wireless LANs</returns>
		/// <remarks>If multiple profiles are associated with a same network, there will be multiple entries
		/// with the same SSID.</remarks>
		public static IEnumerable<NetworkPack> EnumerateAvailableNetworks()
		{
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var availableNetworkList = GetAvailableNetworkList(client.Handle, interfaceInfo.InterfaceGuid);

					foreach (var availableNetwork in availableNetworkList)
					{
						Debug.WriteLine("Interface: {0}, SSID: {1}, Signal: {2}",
							interfaceInfo.strInterfaceDescription,
							availableNetwork.dot11Ssid.ToSsidString(),
							availableNetwork.wlanSignalQuality);

						yield return new NetworkPack(
							interfaceInfo.InterfaceGuid,
							availableNetwork.dot11Ssid.ToSsidString(),
							ConvertToBssType(availableNetwork.dot11BssType),
							(int)availableNetwork.wlanSignalQuality,
							availableNetwork.strProfileName);
					}
				}
			}
		}

		/// <summary>
		/// Enumerate BSSIDs of wireless LANs.
		/// </summary>
		/// <returns>BSSIDs</returns>
		public static IEnumerable<string> EnumerateNetworkBssids()
		{
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var networkBssEntryList = GetNetworkBssEntryList(client.Handle, interfaceInfo.InterfaceGuid);

					foreach (var networkBssEntry in networkBssEntryList)
					{
						var bssid = string.Join(":", networkBssEntry.dot11Bssid.Select(x => x.ToString("X2")));

						Debug.WriteLine("Interface: {0}, SSID: {1} BSSID: {2}",
							interfaceInfo.strInterfaceDescription,
							networkBssEntry.dot11Ssid.ToSsidString(),
							bssid);

						yield return bssid;
					}
				}
			}
		}

		/// <summary>
		/// Enumerate SSIDs of connected wireless LANs.
		/// </summary>
		/// <returns>SSIDs</returns>
		public static IEnumerable<string> EnumerateConnectedNetworkSsids()
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

					Debug.WriteLine("Interface: {0}, SSID: {1}",
						interfaceInfo.strInterfaceDescription,
						association.dot11Ssid.ToSsidString());

					yield return association.dot11Ssid.ToSsidString();
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
						Debug.WriteLine("Interface: {0}, Profile: {1}",
							interfaceInfo.strInterfaceDescription,
							profileInfo.strProfileName);

						yield return profileInfo.strProfileName;
					}
				}
			}
		}

		/// <summary>
		/// Enumerate wireless profiles in preference order.
		/// </summary>
		/// <returns>Wireless profiles</returns>
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
						var signal = (int)availableNetwork.wlanSignalQuality;

						var profileIsConnected = interfaceIsConnected && profileInfo.strProfileName.Equals(connection.strProfileName, StringComparison.Ordinal);

						Debug.WriteLine("Interface: {0}, Profile: {1}, Position: {2}, Signal {3}, IsConnected {4}",
							interfaceInfo.strInterfaceDescription,
							profileInfo.strProfileName,
							position,
							signal,
							profileIsConnected);

						var profile = GetProfile(
							client.Handle,
							profileInfo.strProfileName,
							interfaceInfo.InterfaceGuid,
							interfaceInfo.strInterfaceDescription,
							position++,
							signal,
							profileIsConnected);

						if (profile != null)
							yield return profile;
					}
				}
			}
		}

		/// <summary>
		/// Get a wireless profile.
		/// </summary>
		/// <param name="clientHandle">Client handle</param>
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="interfaceDescription">Interface description</param>
		/// <param name="isConnected">Whether this profile is connected to a wireless LAN</param>
		/// <returns>Wireless profile</returns>
		/// <remarks>
		/// For profile elements, see
		/// https://msdn.microsoft.com/en-us/library/windows/desktop/ms707381.aspx 
		/// </remarks>
		private static ProfilePack GetProfile(SafeClientHandle clientHandle, string profileName, Guid interfaceGuid, string interfaceDescription, int position, int signal, bool isConnected)
		{
			var source = GetProfileXml(clientHandle, interfaceGuid, profileName);
			if (string.IsNullOrWhiteSpace(source))
				return null;

			XElement rootXml;
			using (var sr = new StringReader(source))
				rootXml = XElement.Load(sr);

			var ns = rootXml.Name.Namespace;

			var ssidXml = rootXml.Descendants(ns + "SSID").FirstOrDefault();
			var ssid = (ssidXml != null) ? ssidXml.Descendants(ns + "name").First().Value : null;

			var connectionTypeXml = rootXml.Descendants(ns + "connectionType").FirstOrDefault();
			var bssType = (connectionTypeXml != null) ? ConvertToBssType(connectionTypeXml.Value) : default(BssType);

			var connectionModeXml = rootXml.Descendants(ns + "connectionMode").FirstOrDefault();
			var isAutomatic = (connectionModeXml != null) && connectionModeXml.Value.Equals("auto", StringComparison.OrdinalIgnoreCase);

			var authenticationXml = rootXml.Descendants(ns + "authentication").FirstOrDefault();
			var authentication = (authenticationXml != null) ? authenticationXml.Value : null;

			var encryptionXml = rootXml.Descendants(ns + "encryption").FirstOrDefault();
			var encryption = (encryptionXml != null) ? encryptionXml.Value : null;

			Debug.WriteLine("SSID: {0}, BssType: {1}, Authentication: {2}, Encryption: {3}, IsAutomatic: {4}",
				ssid,
				bssType,
				authentication,
				encryption,
				isAutomatic);

			return new ProfilePack(
				profileName,
				interfaceGuid,
				interfaceDescription,
				ssid,
				bssType,
				authentication,
				encryption,
				position,
				isAutomatic,
				signal,
				isConnected);
		}

		#endregion

		#region Set profile position

		/// <summary>
		/// Set the position of a wireless profile in preference order.
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
		/// Delete a wireless profile.
		/// </summary>
		/// <param name="profileName">Profile name</param>
		/// <returns>True if deleted. False if could not delete.</returns>
		public static bool DeleteProfile(string profileName)
		{
			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var profileInfoList = GetProfileInfoList(client.Handle, interfaceInfo.InterfaceGuid);

					if (!profileInfoList.Any(x => x.strProfileName.Equals(profileName, StringComparison.Ordinal)))
						continue;

					Debug.WriteLine("Existing profile: " + profileName);

					return DeleteProfile(client.Handle, interfaceInfo.InterfaceGuid, profileName);
				}

				return false;
			}
		}

		/// <summary>
		/// Delete a wireless profile.
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
		/// Attempt to connect to a wireless LAN.
		/// </summary>
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="bssType">BSS type</param>
		/// <returns>True if succeeded.</returns>
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
		/// Attempt to connect to a wireless LAN.
		/// </summary>
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="bssType">BSS type</param>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <returns>True if succeeded. False if failed or timed out.</returns>
		public static async Task<bool> ConnectAsync(string profileName, Guid interfaceGuid, BssType bssType, TimeSpan timeoutDuration)
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
				var completedTask = await Task.WhenAny(connectTask, Task.Delay(timeoutDuration));

				return (completedTask == connectTask) ? connectTask.Result : false;
			}
		}

		/// <summary>
		/// Disconnect from a wireless LAN.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <returns>True if succeeded.</returns>
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
		/// Disconnect from a wireless LAN.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="timeoutDuration">Timeout duration</param>
		/// <returns>True if succeeded. False if failed or timed out.</returns>
		public static async Task<bool> DisconnectAsync(Guid interfaceGuid, TimeSpan timeoutDuration)
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
				var completedTask = await Task.WhenAny(disconnectTask, Task.Delay(timeoutDuration));

				return (completedTask == disconnectTask) ? disconnectTask.Result : false;
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

				CheckResult(result, "WlanOpenHandle", true);
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

				return CheckResult(result, "WlanEnumInterfaces", true)
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
			return CheckResult(result, "WlanScan", false);
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
				return CheckResult(result, "WlanGetAvailableNetworkList", false)
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
				return CheckResult(result, "WlanGetNetworkBssList", false)
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
				return CheckResult(result, "WlanQueryInterface", false)
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

				return CheckResult(result, "WlanGetProfileList", false)
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
				return CheckResult(result, "WlanGetProfile", false)
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
			return CheckResult(result, "WlanSetProfilePosition", false);
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
			return CheckResult(result, "WlanDeleteProfile", false);
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
			return CheckResult(result, "WlanConnect", false);
		}

		private static bool Disconnect(SafeClientHandle clientHandle, Guid interfaceGuid)
		{
			var result = WlanDisconnect(
				clientHandle,
				interfaceGuid,
				IntPtr.Zero);

			// ERROR_NOT_FOUND will be returned if the interface is removed.
			return CheckResult(result, "WlanDisconnect", false);
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

			CheckResult(result, "WlanRegisterNotification", true);
		}

		private static WLAN_NOTIFICATION_CALLBACK _notificationCallback;

		private static bool CheckResult(uint result, string methodName, bool willThrowOnFailure)
		{
			if (result == ERROR_SUCCESS)
				return true;

			if (!willThrowOnFailure)
			{
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
						return false;

					case ERROR_INVALID_HANDLE:
					case ERROR_NOT_ENOUGH_MEMORY:
						break;
				}
			}
			throw CreateWin32Exception(result, methodName);
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

			var message = string.Format("Method: {0}, Code: {1}", methodName, errorCode);
			if (0 < result)
				message += string.Format(", Message: {0}", sb.ToString());

			return new Win32Exception((int)errorCode, message);
		}

		#endregion
	}
}