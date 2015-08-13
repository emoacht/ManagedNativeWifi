using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace ManagedNativeWifi
{
	/// <summary>
	/// A managed implementation of Native Wifi API
	/// </summary>
	public class NativeWifi
	{
		#region Win32

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanOpenHandle(
			uint dwClientVersion,
			IntPtr pReserved,
			out uint pdwNegotiatedVersion,
			out IntPtr phClientHandle);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanCloseHandle(
			IntPtr hClientHandle,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern void WlanFreeMemory(IntPtr pMemory);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanEnumInterfaces(
			IntPtr hClientHandle,
			IntPtr pReserved,
			out IntPtr ppInterfaceList);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanGetAvailableNetworkList(
			IntPtr hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			uint dwFlags,
			IntPtr pReserved,
			out IntPtr ppAvailableNetworkList);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanQueryInterface(
			IntPtr hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			WLAN_INTF_OPCODE OpCode,
			IntPtr pReserved,
			out uint pdwDataSize,
			ref IntPtr ppData,
			IntPtr pWlanOpcodeValueType);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanGetProfileList(
			IntPtr hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			IntPtr pReserved,
			out IntPtr ppProfileList);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanGetProfile(
			IntPtr hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			[MarshalAs(UnmanagedType.LPWStr)] string strProfileName,
			IntPtr pReserved,
			out IntPtr pstrProfileXml,
			ref uint pdwFlags,
			out uint pdwGrantedAccess);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanSetProfile(
			IntPtr hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			uint dwFlags,
			[MarshalAs(UnmanagedType.LPWStr)] string strProfileXml,
			[MarshalAs(UnmanagedType.LPWStr)] string strAllUserProfileSecurity,
			[MarshalAs(UnmanagedType.Bool)] bool bOverwrite,
			IntPtr pReserved,
			out uint pdwReasonCode);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanSetProfilePosition(
			IntPtr hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			[MarshalAs(UnmanagedType.LPWStr)] string strProfileName,
			uint dwPosition,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanDeleteProfile(
			IntPtr hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			[MarshalAs(UnmanagedType.LPWStr)] string strProfileName,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanConnect(
			IntPtr hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			[In] ref WLAN_CONNECTION_PARAMETERS pConnectionParameters,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll", SetLastError = true)]
		private static extern uint WlanDisconnect(
			IntPtr hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			IntPtr pReserved);

		[DllImport("Kernel32.dll", SetLastError = true)]
		private static extern uint FormatMessage(
			uint dwFlags,
			IntPtr lpSource,
			uint dwMessageId,
			uint dwLanguageId,
			StringBuilder lpBuffer,
			int nSize,
			IntPtr Arguments);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WLAN_INTERFACE_INFO
		{
			public Guid InterfaceGuid;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strInterfaceDescription;

			public WLAN_INTERFACE_STATE isState;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WLAN_INTERFACE_INFO_LIST
		{
			public uint dwNumberOfItems;
			public uint dwIndex;
			public WLAN_INTERFACE_INFO[] InterfaceInfo;

			public WLAN_INTERFACE_INFO_LIST(IntPtr ppInterfaceList)
			{
				dwNumberOfItems = (uint)Marshal.ReadInt32(ppInterfaceList, 0);
				dwIndex = (uint)Marshal.ReadInt32(ppInterfaceList, 4);
				InterfaceInfo = new WLAN_INTERFACE_INFO[dwNumberOfItems];

				var offset = Marshal.SizeOf(typeof(uint)) * 2; // Size of dwNumberOfItems and dwIndex

				for (int i = 0; i < dwNumberOfItems; i++)
				{
					var interfaceInfo = new IntPtr(ppInterfaceList.ToInt64() + (Marshal.SizeOf(typeof(WLAN_INTERFACE_INFO)) * i) + offset);
					InterfaceInfo[i] = (WLAN_INTERFACE_INFO)Marshal.PtrToStructure(interfaceInfo, typeof(WLAN_INTERFACE_INFO));
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WLAN_AVAILABLE_NETWORK
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strProfileName;

			public DOT11_SSID dot11Ssid;
			public DOT11_BSS_TYPE dot11BssType;
			public uint uNumberOfBssids;
			public bool bNetworkConnectable;
			public uint wlanNotConnectableReason;
			public uint uNumberOfPhyTypes;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public DOT11_PHY_TYPE[] dot11PhyTypes;

			public bool bMorePhyTypes;
			public uint wlanSignalQuality;
			public bool bSecurityEnabled;
			public DOT11_AUTH_ALGORITHM dot11DefaultAuthAlgorithm;
			public DOT11_CIPHER_ALGORITHM dot11DefaultCipherAlgorithm;
			public uint dwFlags;
			public uint dwReserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WLAN_AVAILABLE_NETWORK_LIST
		{
			public uint dwNumberOfItems;
			public uint dwIndex;
			public WLAN_AVAILABLE_NETWORK[] Network;

			public WLAN_AVAILABLE_NETWORK_LIST(IntPtr ppAvailableNetworkList)
			{
				dwNumberOfItems = (uint)Marshal.ReadInt32(ppAvailableNetworkList, 0);
				dwIndex = (uint)Marshal.ReadInt32(ppAvailableNetworkList, 4);
				Network = new WLAN_AVAILABLE_NETWORK[dwNumberOfItems];

				var offset = Marshal.SizeOf(typeof(uint)) * 2; // Size of dwNumberOfItems and dwIndex

				for (int i = 0; i < dwNumberOfItems; i++)
				{
					var availableNetwork = new IntPtr(ppAvailableNetworkList.ToInt64() + (Marshal.SizeOf(typeof(WLAN_AVAILABLE_NETWORK)) * i) + offset);
					Network[i] = (WLAN_AVAILABLE_NETWORK)Marshal.PtrToStructure(availableNetwork, typeof(WLAN_AVAILABLE_NETWORK));
				}
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DOT11_SSID
		{
			public uint uSSIDLength;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] ucSSID;

			public byte[] ToSsidBytes()
			{
				return (ucSSID != null)
					? ucSSID.Take((int)uSSIDLength).ToArray()
					: null;
			}

			public string ToSsidString()
			{
				return (ucSSID != null)
					? Encoding.UTF8.GetString(ToSsidBytes())
					: null;
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WLAN_CONNECTION_ATTRIBUTES
		{
			public WLAN_INTERFACE_STATE isState;
			public WLAN_CONNECTION_MODE wlanConnectionMode;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strProfileName;

			public WLAN_ASSOCIATION_ATTRIBUTES wlanAssociationAttributes;
			public WLAN_SECURITY_ATTRIBUTES wlanSecurityAttributes;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WLAN_ASSOCIATION_ATTRIBUTES
		{
			public DOT11_SSID dot11Ssid;
			public DOT11_BSS_TYPE dot11BssType;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] dot11Bssid; // DOT11_MAC_ADDRESS

			public DOT11_PHY_TYPE dot11PhyType;
			public uint uDot11PhyIndex;
			public uint wlanSignalQuality;
			public uint ulRxRate;
			public uint ulTxRate;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WLAN_SECURITY_ATTRIBUTES
		{
			[MarshalAs(UnmanagedType.Bool)]
			public bool bSecurityEnabled;

			[MarshalAs(UnmanagedType.Bool)]
			public bool bOneXEnabled;

			public DOT11_AUTH_ALGORITHM dot11AuthAlgorithm;
			public DOT11_CIPHER_ALGORITHM dot11CipherAlgorithm;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WLAN_PROFILE_INFO
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strProfileName;

			public uint dwFlags;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WLAN_PROFILE_INFO_LIST
		{
			public uint dwNumberOfItems;
			public uint dwIndex;
			public WLAN_PROFILE_INFO[] ProfileInfo;

			public WLAN_PROFILE_INFO_LIST(IntPtr ppProfileList)
			{
				dwNumberOfItems = (uint)Marshal.ReadInt32(ppProfileList, 0);
				dwIndex = (uint)Marshal.ReadInt32(ppProfileList, 4);
				ProfileInfo = new WLAN_PROFILE_INFO[dwNumberOfItems];

				var offset = Marshal.SizeOf(typeof(uint)) * 2; // Size of dwNumberOfItems and dwIndex

				for (int i = 0; i < dwNumberOfItems; i++)
				{
					var profileInfo = new IntPtr(ppProfileList.ToInt64() + (Marshal.SizeOf(typeof(WLAN_PROFILE_INFO)) * i) + offset);
					ProfileInfo[i] = (WLAN_PROFILE_INFO)Marshal.PtrToStructure(profileInfo, typeof(WLAN_PROFILE_INFO));
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WLAN_CONNECTION_PARAMETERS
		{
			public WLAN_CONNECTION_MODE wlanConnectionMode;
			[MarshalAs(UnmanagedType.LPWStr)]
			public string strProfile;
			public IntPtr pDot11Ssid; // DOT11_SSID[]
			public IntPtr pDesiredBssidList; // DOT11_BSSID_LIST[]
			public DOT11_BSS_TYPE dot11BssType;
			public uint dwFlags;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct DOT11_BSSID_LIST
		{
			public NDIS_OBJECT_HEADER Header;
			public uint uNumOfEntries;
			public uint uTotalNumOfEntries;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] BSSIDs; // DOT11_MAC_ADDRESS
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NDIS_OBJECT_HEADER
		{
			public byte Type;
			public byte Revision;
			public ushort Size;
		}

		private enum WLAN_INTERFACE_STATE
		{
			wlan_interface_state_not_ready = 0,
			wlan_interface_state_connected = 1,
			wlan_interface_state_ad_hoc_network_formed = 2,
			wlan_interface_state_disconnecting = 3,
			wlan_interface_state_disconnected = 4,
			wlan_interface_state_associating = 5,
			wlan_interface_state_discovering = 6,
			wlan_interface_state_authenticating = 7
		}

		private enum WLAN_CONNECTION_MODE
		{
			wlan_connection_mode_profile,
			wlan_connection_mode_temporary_profile,
			wlan_connection_mode_discovery_secure,
			wlan_connection_mode_discovery_unsecure,
			wlan_connection_mode_auto,
			wlan_connection_mode_invalid
		}

		private enum DOT11_BSS_TYPE
		{
			/// <summary>
			/// Infrastructure BSS network
			/// </summary>
			dot11_BSS_type_infrastructure = 1,

			/// <summary>
			/// Independent BSS (IBSS) network
			/// </summary>
			dot11_BSS_type_independent = 2,

			/// <summary>
			/// Either infrastructure or IBSS network
			/// </summary>
			dot11_BSS_type_any = 3,
		}

		private enum DOT11_PHY_TYPE : uint
		{
			dot11_phy_type_unknown = 0,
			dot11_phy_type_any = 0,
			dot11_phy_type_fhss = 1,
			dot11_phy_type_dsss = 2,
			dot11_phy_type_irbaseband = 3,
			dot11_phy_type_ofdm = 4,
			dot11_phy_type_hrdsss = 5,
			dot11_phy_type_erp = 6,
			dot11_phy_type_ht = 7,
			dot11_phy_type_vht = 8,
			dot11_phy_type_IHV_start = 0x80000000,
			dot11_phy_type_IHV_end = 0xffffffff
		}

		private enum DOT11_AUTH_ALGORITHM : uint
		{
			DOT11_AUTH_ALGO_80211_OPEN = 1,
			DOT11_AUTH_ALGO_80211_SHARED_KEY = 2,
			DOT11_AUTH_ALGO_WPA = 3,
			DOT11_AUTH_ALGO_WPA_PSK = 4,
			DOT11_AUTH_ALGO_WPA_NONE = 5,
			DOT11_AUTH_ALGO_RSNA = 6,
			DOT11_AUTH_ALGO_RSNA_PSK = 7,
			DOT11_AUTH_ALGO_IHV_START = 0x80000000,
			DOT11_AUTH_ALGO_IHV_END = 0xffffffff
		}

		private enum DOT11_CIPHER_ALGORITHM : uint
		{
			DOT11_CIPHER_ALGO_NONE = 0x00,
			DOT11_CIPHER_ALGO_WEP40 = 0x01,
			DOT11_CIPHER_ALGO_TKIP = 0x02,
			DOT11_CIPHER_ALGO_CCMP = 0x04,
			DOT11_CIPHER_ALGO_WEP104 = 0x05,
			DOT11_CIPHER_ALGO_WPA_USE_GROUP = 0x100,
			DOT11_CIPHER_ALGO_RSN_USE_GROUP = 0x100,
			DOT11_CIPHER_ALGO_WEP = 0x101,
			DOT11_CIPHER_ALGO_IHV_START = 0x80000000,
			DOT11_CIPHER_ALGO_IHV_END = 0xffffffff
		}

		private enum WLAN_INTF_OPCODE : uint
		{
			wlan_intf_opcode_autoconf_start = 0x000000000,
			wlan_intf_opcode_autoconf_enabled,
			wlan_intf_opcode_background_scan_enabled,
			wlan_intf_opcode_media_streaming_mode,
			wlan_intf_opcode_radio_state,
			wlan_intf_opcode_bss_type,
			wlan_intf_opcode_interface_state,
			wlan_intf_opcode_current_connection,
			wlan_intf_opcode_channel_number,
			wlan_intf_opcode_supported_infrastructure_auth_cipher_pairs,
			wlan_intf_opcode_supported_adhoc_auth_cipher_pairs,
			wlan_intf_opcode_supported_country_or_region_string_list,
			wlan_intf_opcode_current_operation_mode,
			wlan_intf_opcode_supported_safe_mode,
			wlan_intf_opcode_certified_safe_mode,
			wlan_intf_opcode_hosted_network_capable,
			wlan_intf_opcode_management_frame_protection_capable,
			wlan_intf_opcode_autoconf_end = 0x0fffffff,
			wlan_intf_opcode_msm_start = 0x10000100,
			wlan_intf_opcode_statistics,
			wlan_intf_opcode_rssi,
			wlan_intf_opcode_msm_end = 0x1fffffff,
			wlan_intf_opcode_security_start = 0x20010000,
			wlan_intf_opcode_security_end = 0x2fffffff,
			wlan_intf_opcode_ihv_start = 0x30000000,
			wlan_intf_opcode_ihv_end = 0x3fffffff
		}

		private const uint WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_ADHOC_PROFILES = 0x00000001;
		private const uint WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_MANUAL_HIDDEN_PROFILES = 0x00000002;

		private const uint ERROR_SUCCESS = 0;
		private const uint ERROR_INVALID_PARAMETER = 87;
		private const uint ERROR_INVALID_HANDLE = 6;
		private const uint ERROR_NOT_ENOUGH_MEMORY = 8;
		private const uint ERROR_ACCESS_DENIED = 5;
		private const uint ERROR_NOT_FOUND = 1168;
		private const uint ERROR_REMOTE_SESSION_LIMIT_EXCEEDED = 1220;
		private const uint ERROR_INVALID_STATE = 5023;
		private const uint ERROR_NDIS_DOT11_MEDIA_IN_USE = 0x80342001;
		private const uint ERROR_NDIS_DOT11_POWER_STATE_INVALID = 0x80342002;

		private const uint WLAN_PROFILE_GET_PLAINTEXT_KEY = 0x00000004;
		private const uint WLAN_PROFILE_GROUP_POLICY = 0x00000001;
		private const uint WLAN_PROFILE_USER = 0x00000002;

		private const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

		#endregion

		#region Original method

		/// <summary>
		/// Get SSIDs of available Wi-Fi networks.
		/// </summary>
		/// <returns>SSIDs</returns>
		public static IEnumerable<string> GetAvailableNetworkSsids()
		{
			var clientHandle = IntPtr.Zero;
			var interfaceList = IntPtr.Zero;
			var availableNetworkList = IntPtr.Zero;

			try
			{
				uint negotiatedVersion;
				if (WlanOpenHandle(
					2, // Client version for Windows Vista and Windows Server 2008
					IntPtr.Zero,
					out negotiatedVersion,
					out clientHandle) != ERROR_SUCCESS)
					yield break;

				if (WlanEnumInterfaces(
					clientHandle,
					IntPtr.Zero,
					out interfaceList) != ERROR_SUCCESS)
					yield break;

				var interfaceInfoList = new WLAN_INTERFACE_INFO_LIST(interfaceList);

				Debug.WriteLine("Interface count: {0}", interfaceInfoList.dwNumberOfItems);

				for (int i = 0; i < interfaceInfoList.dwNumberOfItems; i++)
				{
					var interfaceGuid = interfaceInfoList.InterfaceInfo[i].InterfaceGuid;

					if (WlanGetAvailableNetworkList(
						clientHandle,
						interfaceGuid,
						WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_MANUAL_HIDDEN_PROFILES,
						IntPtr.Zero,
						out availableNetworkList) != ERROR_SUCCESS)
						continue;

					var networkList = new WLAN_AVAILABLE_NETWORK_LIST(availableNetworkList);

					for (int j = 0; j < networkList.dwNumberOfItems; j++)
					{
						var network = networkList.Network[j];

						Debug.WriteLine("Interface: {0}, SSID: {1}, Quality: {2}",
							interfaceInfoList.InterfaceInfo[i].strInterfaceDescription,
							network.dot11Ssid.ToSsidString(),
							network.wlanSignalQuality);

						yield return network.dot11Ssid.ToSsidString();
					}
				}
			}
			finally
			{
				if (availableNetworkList != IntPtr.Zero)
					WlanFreeMemory(availableNetworkList);

				if (interfaceList != IntPtr.Zero)
					WlanFreeMemory(interfaceList);

				if (clientHandle != IntPtr.Zero)
					WlanCloseHandle(clientHandle, IntPtr.Zero);
			}
		}

		/// <summary>
		/// Get SSIDs of connected Wi-Fi networks.
		/// </summary>
		/// <returns>SSIDs</returns>
		public static IEnumerable<string> GetConnectedNetworkSsids()
		{
			var clientHandle = IntPtr.Zero;
			var interfaceList = IntPtr.Zero;
			var queryData = IntPtr.Zero;

			try
			{
				uint negotiatedVersion;
				if (WlanOpenHandle(
					2, // Client version for Windows Vista and Windows Server 2008
					IntPtr.Zero,
					out negotiatedVersion,
					out clientHandle) != ERROR_SUCCESS)
					yield break;

				if (WlanEnumInterfaces(
					clientHandle,
					IntPtr.Zero,
					out interfaceList) != ERROR_SUCCESS)
					yield break;

				var interfaceInfoList = new WLAN_INTERFACE_INFO_LIST(interfaceList);

				Debug.WriteLine("Interface count: {0}", interfaceInfoList.dwNumberOfItems);

				for (int i = 0; i < interfaceInfoList.dwNumberOfItems; i++)
				{
					var interfaceGuid = interfaceInfoList.InterfaceInfo[i].InterfaceGuid;

					uint dataSize;
					if (WlanQueryInterface(
						clientHandle,
						interfaceGuid,
						WLAN_INTF_OPCODE.wlan_intf_opcode_current_connection,
						IntPtr.Zero,
						out dataSize,
						ref queryData,
						IntPtr.Zero) != ERROR_SUCCESS) // If not connected to a network, ERROR_INVALID_STATE will be returned.
						continue;

					var connection = (WLAN_CONNECTION_ATTRIBUTES)Marshal.PtrToStructure(queryData, typeof(WLAN_CONNECTION_ATTRIBUTES));
					if (connection.isState != WLAN_INTERFACE_STATE.wlan_interface_state_connected)
						continue;

					var association = connection.wlanAssociationAttributes;

					Debug.WriteLine("Interface: {0}, SSID: {1}, Quality: {2}",
						interfaceInfoList.InterfaceInfo[i].strInterfaceDescription,
						association.dot11Ssid.ToSsidString(),
						association.wlanSignalQuality);

					yield return association.dot11Ssid.ToSsidString();
				}
			}
			finally
			{
				if (queryData != IntPtr.Zero)
					WlanFreeMemory(queryData);

				if (interfaceList != IntPtr.Zero)
					WlanFreeMemory(interfaceList);

				if (clientHandle != IntPtr.Zero)
					WlanCloseHandle(clientHandle, IntPtr.Zero);
			}
		}

		#endregion

		#region Revised method

		#region Type

		public enum BssType
		{
			None = 0,
			Infrastructure,
			Independent,
			Any
		}

		public class ProfilePack
		{
			public string Name { get; private set; }
			public Guid InterfaceGuid { get; private set; }
			public string InterfaceDescription { get; private set; }
			public string Ssid { get; private set; }
			public BssType BssType { get; private set; }
			public string Authentication { get; private set; }
			public string Encryption { get; private set; }
			public int Position { get; private set; }
			public bool IsAutomatic { get; private set; }
			public int Signal { get; private set; }
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

		public class NetworkPack
		{
			public Guid InterfaceGuid { get; private set; }
			public string Ssid { get; private set; }
			public BssType BssType { get; private set; }
			public int Signal { get; private set; }
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
		private static ProfilePack GetProfile(IntPtr clientHandle, string profileName, Guid interfaceGuid, string interfaceDescription, int position, int signal, bool isConnected)
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
				return false;

			if (interfaceGuid == default(Guid))
				return false;

			if (position < 0)
				return false;

			using (var client = new WlanClient())
			{
				return SetProfilePosition(client.Handle, interfaceGuid, profileName, (uint)position);
			}
		}

		/// <summary>
		/// Delete a wireless profile.
		/// </summary>
		/// <param name="profileName">Profile name</param>
		/// <returns>True if deleted. False if could not delete.</returns>
		public static bool DeleteProfile(string profileName)
		{
			if (string.IsNullOrWhiteSpace(profileName))
				return false;

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
				return false;

			if (interfaceGuid == default(Guid))
				return false;

			using (var client = new WlanClient())
			{
				return DeleteProfile(client.Handle, interfaceGuid, profileName);
			}
		}

		/// <summary>
		/// Attempt to connect to a wireless LAN.
		/// </summary>
		/// <param name="profileName">Profile name</param>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <param name="bssType">BSS type</param>
		/// <returns>True if the function succeeded.</returns>
		public static bool Connect(string profileName, Guid interfaceGuid, BssType bssType = BssType.Any)
		{
			if (string.IsNullOrWhiteSpace(profileName))
				return false;

			if (interfaceGuid == default(Guid))
				return false;

			using (var client = new WlanClient())
			{
				return Connect(client.Handle, interfaceGuid, profileName, ConvertFromBssType(bssType));
			}
		}

		/// <summary>
		/// Disconnect from a wireless LAN.
		/// </summary>
		/// <param name="interfaceGuid">Interface GUID</param>
		/// <returns>True if the function succeeded.</returns>
		public static bool Disconnect(Guid interfaceGuid)
		{
			if (interfaceGuid == default(Guid))
				return false;

			using (var client = new WlanClient())
			{
				return Disconnect(client.Handle, interfaceGuid);
			}
		}

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
			private IntPtr _clientHandle = IntPtr.Zero;

			public IntPtr Handle { get { return _clientHandle; } }

			public WlanClient()
			{
				uint negotiatedVersion;
				var result = WlanOpenHandle(
					2, // Client version for Windows Vista and Windows Server 2008
					IntPtr.Zero,
					out negotiatedVersion,
					out _clientHandle);

				switch (result)
				{
					case ERROR_SUCCESS:
						break;
					case ERROR_INVALID_PARAMETER:
					case ERROR_NOT_ENOUGH_MEMORY:
					case ERROR_REMOTE_SESSION_LIMIT_EXCEEDED:
					default:
						throw CreateWin32Exception(result, "WlanOpenHandle");
				}
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

				if (_clientHandle != IntPtr.Zero)
					WlanCloseHandle(_clientHandle, IntPtr.Zero);

				_disposed = true;
			}

			~WlanClient()
			{
				Dispose(false);
			}

			#endregion
		}

		private static WLAN_INTERFACE_INFO[] GetInterfaceInfoList(IntPtr clientHandle)
		{
			var interfaceList = IntPtr.Zero;
			try
			{
				var result = WlanEnumInterfaces(
					clientHandle,
					IntPtr.Zero,
					out interfaceList);

				switch (result)
				{
					case ERROR_SUCCESS:
						return new WLAN_INTERFACE_INFO_LIST(interfaceList).InterfaceInfo;
					case ERROR_INVALID_PARAMETER:
					case ERROR_INVALID_HANDLE:
					case ERROR_NOT_ENOUGH_MEMORY:
					default:
						throw CreateWin32Exception(result, "WlanEnumInterfaces");
				}
			}
			finally
			{
				if (interfaceList != IntPtr.Zero)
					WlanFreeMemory(interfaceList);
			}
		}

		private static WLAN_AVAILABLE_NETWORK[] GetAvailableNetworkList(IntPtr clientHandle, Guid interfaceGuid)
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

				switch (result)
				{
					case ERROR_SUCCESS:
						return new WLAN_AVAILABLE_NETWORK_LIST(availableNetworkList).Network;
					case ERROR_NDIS_DOT11_POWER_STATE_INVALID: // If the interface is turned off, this value will be returned. 
					case ERROR_INVALID_PARAMETER:
						return new WLAN_AVAILABLE_NETWORK[] { };
					case ERROR_INVALID_HANDLE:
					case ERROR_NOT_ENOUGH_MEMORY:
					default:
						throw CreateWin32Exception(result, "WlanGetAvailableNetworkList");
				}
			}
			finally
			{
				if (availableNetworkList != IntPtr.Zero)
					WlanFreeMemory(availableNetworkList);
			}
		}

		private static WLAN_CONNECTION_ATTRIBUTES GetConnectionAttributes(IntPtr clientHandle, Guid interfaceGuid)
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

				switch (result)
				{
					case ERROR_SUCCESS:
						return (WLAN_CONNECTION_ATTRIBUTES)Marshal.PtrToStructure(queryData, typeof(WLAN_CONNECTION_ATTRIBUTES));
					case ERROR_INVALID_STATE: // If not connected to a network, this value will be returned.
					case ERROR_INVALID_PARAMETER:
					case ERROR_ACCESS_DENIED:
						return default(WLAN_CONNECTION_ATTRIBUTES);
					case ERROR_INVALID_HANDLE:
					case ERROR_NOT_ENOUGH_MEMORY:
					default:
						throw CreateWin32Exception(result, "WlanQueryInterface");
				}
			}
			finally
			{
				if (queryData != IntPtr.Zero)
					WlanFreeMemory(queryData);
			}
		}

		private static WLAN_PROFILE_INFO[] GetProfileInfoList(IntPtr clientHandle, Guid interfaceGuid)
		{
			var profileList = IntPtr.Zero;
			try
			{
				var result = WlanGetProfileList(
					clientHandle,
					interfaceGuid,
					IntPtr.Zero,
					out profileList);

				switch (result)
				{
					case ERROR_SUCCESS:
						return new WLAN_PROFILE_INFO_LIST(profileList).ProfileInfo;
					case ERROR_INVALID_PARAMETER:
						return new WLAN_PROFILE_INFO[] { };
					case ERROR_INVALID_HANDLE:
					case ERROR_NOT_ENOUGH_MEMORY:
					default:
						throw CreateWin32Exception(result, "WlanGetProfileList");
				}
			}
			finally
			{
				if (profileList != IntPtr.Zero)
					WlanFreeMemory(profileList);
			}
		}

		private static string GetProfileXml(IntPtr clientHandle, Guid interfaceGuid, string profileName)
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

				switch (result)
				{
					case ERROR_SUCCESS:
						return Marshal.PtrToStringUni(profileXml);
					case ERROR_NOT_FOUND: // If the profile is not found, this value will be returned.
					case ERROR_INVALID_PARAMETER:
					case ERROR_ACCESS_DENIED:
						return null;
					case ERROR_INVALID_HANDLE:
					case ERROR_NOT_ENOUGH_MEMORY:
					default:
						throw CreateWin32Exception(result, "WlanGetProfile");
				}
			}
			finally
			{
				if (profileXml != IntPtr.Zero)
					WlanFreeMemory(profileXml);
			}
		}

		private static bool SetProfilePosition(IntPtr clientHandle, Guid interfaceGuid, string profileName, uint position)
		{
			var result = WlanSetProfilePosition(
				clientHandle,
				interfaceGuid,
				profileName,
				position,
				IntPtr.Zero);

			switch (result)
			{
				case ERROR_SUCCESS:
					return true;
				case ERROR_NOT_FOUND: // If the position is invalid, this value will be returned.
				case ERROR_INVALID_PARAMETER: // If the interface has been removed, this value will be returned.
				case ERROR_ACCESS_DENIED:
					return false;
				case ERROR_INVALID_HANDLE:
				default:
					throw CreateWin32Exception(result, "WlanSetProfilePosition");
			}
		}

		private static bool DeleteProfile(IntPtr clientHandle, Guid interfaceGuid, string profileName)
		{
			var result = WlanDeleteProfile(
				clientHandle,
				interfaceGuid,
				profileName,
				IntPtr.Zero);

			switch (result)
			{
				case ERROR_SUCCESS:
					return true;
				case ERROR_NOT_FOUND: // If the profile is not found, this value will be returned.
				case ERROR_INVALID_PARAMETER: // If the interface has been removed, this value will be returned.
				case ERROR_ACCESS_DENIED:
					return false;
				case ERROR_INVALID_HANDLE:
				default:
					throw CreateWin32Exception(result, "WlanDeleteProfile");
			}
		}

		private static bool Connect(IntPtr clientHandle, Guid interfaceGuid, string profileName, DOT11_BSS_TYPE bssType)
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

			switch (result)
			{
				case ERROR_SUCCESS:
					return true;
				case ERROR_INVALID_PARAMETER:
				case ERROR_ACCESS_DENIED:
					return false;
				case ERROR_INVALID_HANDLE:
				default:
					throw CreateWin32Exception(result, "WlanConnect");
			}
		}

		private static bool Disconnect(IntPtr clientHandle, Guid interfaceGuid)
		{
			var result = WlanDisconnect(
				clientHandle,
				interfaceGuid,
				IntPtr.Zero);

			switch (result)
			{
				case ERROR_SUCCESS:
					return true;
				case ERROR_INVALID_PARAMETER:
				case ERROR_ACCESS_DENIED:
					return false;
				case ERROR_INVALID_HANDLE:
				case ERROR_NOT_ENOUGH_MEMORY:
				default:
					throw CreateWin32Exception(result, "WlanDisconnect");
			}
		}

		private static Win32Exception CreateWin32Exception(uint errorCode, string methodName)
		{
			var sb = new StringBuilder(512);

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

		#endregion
	}
}