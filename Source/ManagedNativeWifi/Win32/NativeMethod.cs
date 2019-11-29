using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi.Win32
{
	internal static class NativeMethod
	{
		#region Method

		[SuppressUnmanagedCodeSecurity]
		[DllImport("Wlanapi.dll")]
		public static extern uint WlanOpenHandle(
			uint dwClientVersion,
			IntPtr pReserved,
			out uint pdwNegotiatedVersion,
			out SafeClientHandle phClientHandle);

		[SuppressUnmanagedCodeSecurity]
		[DllImport("Wlanapi.dll")]
		public static extern uint WlanCloseHandle(
			IntPtr hClientHandle,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll")]
		public static extern void WlanFreeMemory(IntPtr pMemory);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanEnumInterfaces(
			SafeClientHandle hClientHandle,
			IntPtr pReserved,
			out IntPtr ppInterfaceList); // Pointer to WLAN_INTERFACE_INFO_LIST

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanQueryInterface(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			WLAN_INTF_OPCODE OpCode,
			IntPtr pReserved,
			out uint pdwDataSize,
			out IntPtr ppData, // Pointer to queried data
			IntPtr pWlanOpcodeValueType);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanGetInterfaceCapability(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			IntPtr pReserved,
			out IntPtr ppCapability); // Pointer to WLAN_INTERFACE_CAPABILITY

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanSetInterface(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			WLAN_INTF_OPCODE OpCode,
			uint dwDataSize,
			IntPtr pData, // Pointer to data to be set
			IntPtr pReserved);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanScan(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			IntPtr pDot11Ssid,
			IntPtr pIeData,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanGetAvailableNetworkList(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			uint dwFlags,
			IntPtr pReserved,
			out IntPtr ppAvailableNetworkList); // Pointer to WLAN_AVAILABLE_NETWORK_LIST

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanGetNetworkBssList(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			IntPtr pDot11Ssid,
			DOT11_BSS_TYPE dot11BssType,
			[MarshalAs(UnmanagedType.Bool)] bool bSecurityEnabled,
			IntPtr pReserved,
			out IntPtr ppWlanBssList); // Pointer to WLAN_BSS_LIST

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanGetProfileList(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			IntPtr pReserved,
			out IntPtr ppProfileList); // Pointer to WLAN_PROFILE_INFO_LIST

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanGetProfile(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			[MarshalAs(UnmanagedType.LPWStr)] string strProfileName,
			IntPtr pReserved,
			[MarshalAs(UnmanagedType.LPWStr)] out string pstrProfileXml,
			ref uint pdwFlags,
			out uint pdwGrantedAccess);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanSetProfile(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			uint dwFlags,
			[MarshalAs(UnmanagedType.LPWStr)] string strProfileXml,
			[MarshalAs(UnmanagedType.LPWStr)] string strAllUserProfileSecurity,
			[MarshalAs(UnmanagedType.Bool)] bool bOverwrite,
			IntPtr pReserved,
			out uint pdwReasonCode); // WLAN_REASON_CODE

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanSetProfilePosition(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			[MarshalAs(UnmanagedType.LPWStr)] string strProfileName,
			uint dwPosition,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanRenameProfile(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			[MarshalAs(UnmanagedType.LPWStr)] string strOldProfileName,
			[MarshalAs(UnmanagedType.LPWStr)] string strNewProfileName,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanDeleteProfile(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			[MarshalAs(UnmanagedType.LPWStr)] string strProfileName,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanConnect(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			[In] ref WLAN_CONNECTION_PARAMETERS pConnectionParameters,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanDisconnect(
			SafeClientHandle hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanRegisterNotification(
			SafeClientHandle hClientHandle,
			uint dwNotifSource,
			[MarshalAs(UnmanagedType.Bool)] bool bIgnoreDuplicate,
			WLAN_NOTIFICATION_CALLBACK funcCallback,
			IntPtr pCallbackContext,
			IntPtr pReserved,
			uint pdwPrevNotifSource);

		public delegate void WLAN_NOTIFICATION_CALLBACK(
			IntPtr data, // Pointer to WLAN_NOTIFICATION_DATA
			IntPtr context);

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanReasonCodeToString(
			uint dwReasonCode,
			int dwBufferSize,
			[MarshalAs(UnmanagedType.LPWStr)] StringBuilder pStringBuffer,
			IntPtr pReserved);

		[DllImport("Kernel32.dll", SetLastError = true)]
		public static extern uint FormatMessage(
			uint dwFlags,
			IntPtr lpSource,
			uint dwMessageId,
			uint dwLanguageId,
			StringBuilder lpBuffer,
			int nSize,
			IntPtr Arguments);

		#endregion

		#region Struct (Primary)

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WLAN_INTERFACE_INFO
		{
			public Guid InterfaceGuid;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strInterfaceDescription;

			public WLAN_INTERFACE_STATE isState;
		}

		public struct WLAN_INTERFACE_INFO_LIST
		{
			public uint dwNumberOfItems;
			public uint dwIndex;
			public WLAN_INTERFACE_INFO[] InterfaceInfo;

			public WLAN_INTERFACE_INFO_LIST(IntPtr ppInterfaceList)
			{
				var uintSize = Marshal.SizeOf<uint>(); // 4

				dwNumberOfItems = (uint)Marshal.ReadInt32(ppInterfaceList, 0);
				dwIndex = (uint)Marshal.ReadInt32(ppInterfaceList, uintSize /* Offset for dwNumberOfItems */);
				InterfaceInfo = new WLAN_INTERFACE_INFO[dwNumberOfItems];

				for (int i = 0; i < dwNumberOfItems; i++)
				{
					var interfaceInfo = new IntPtr(ppInterfaceList.ToInt64()
						+ (uintSize * 2) /* Offset for dwNumberOfItems and dwIndex */
						+ (Marshal.SizeOf<WLAN_INTERFACE_INFO>() * i) /* Offset for preceding items */);

					InterfaceInfo[i] = Marshal.PtrToStructure<WLAN_INTERFACE_INFO>(interfaceInfo);
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WLAN_CONNECTION_ATTRIBUTES
		{
			public WLAN_INTERFACE_STATE isState;
			public WLAN_CONNECTION_MODE wlanConnectionMode;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strProfileName;

			public WLAN_ASSOCIATION_ATTRIBUTES wlanAssociationAttributes;
			public WLAN_SECURITY_ATTRIBUTES wlanSecurityAttributes;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WLAN_INTERFACE_CAPABILITY
		{
			public WLAN_INTERFACE_TYPE interfaceType;

			[MarshalAs(UnmanagedType.Bool)]
			public bool bDot11DSupported;

			public uint dwMaxDesiredSsidListSize;
			public uint dwMaxDesiredBssidListSize;
			public uint dwNumberOfSupportedPhys;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
			public DOT11_PHY_TYPE[] dot11PhyTypes;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WLAN_PHY_RADIO_STATE
		{
			public uint dwPhyIndex;
			public DOT11_RADIO_STATE dot11SoftwareRadioState;
			public DOT11_RADIO_STATE dot11HardwareRadioState;
		}

		public struct WLAN_RADIO_STATE
		{
			public uint dwNumberOfPhys;
			public WLAN_PHY_RADIO_STATE[] PhyRadioState;

			public WLAN_RADIO_STATE(IntPtr ppData)
			{
				var uintSize = Marshal.SizeOf<uint>(); // 4

				dwNumberOfPhys = (uint)Marshal.ReadInt32(ppData, 0);
				PhyRadioState = new WLAN_PHY_RADIO_STATE[dwNumberOfPhys];

				for (int i = 0; i < dwNumberOfPhys; i++)
				{
					var phyRadioState = new IntPtr(ppData.ToInt64()
						+ uintSize /* Offset for dwNumberOfPhys */
						+ (Marshal.SizeOf<WLAN_PHY_RADIO_STATE>() * i) /* Offset for preceding items */);

					PhyRadioState[i] = Marshal.PtrToStructure<WLAN_PHY_RADIO_STATE>(phyRadioState);
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WLAN_AVAILABLE_NETWORK
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

		public struct WLAN_AVAILABLE_NETWORK_LIST
		{
			public uint dwNumberOfItems;
			public uint dwIndex;
			public WLAN_AVAILABLE_NETWORK[] Network;

			public WLAN_AVAILABLE_NETWORK_LIST(IntPtr ppAvailableNetworkList)
			{
				var uintSize = Marshal.SizeOf<uint>(); // 4

				dwNumberOfItems = (uint)Marshal.ReadInt32(ppAvailableNetworkList, 0);
				dwIndex = (uint)Marshal.ReadInt32(ppAvailableNetworkList, uintSize /* Offset for dwNumberOfItems */);
				Network = new WLAN_AVAILABLE_NETWORK[dwNumberOfItems];

				for (int i = 0; i < dwNumberOfItems; i++)
				{
					var availableNetwork = new IntPtr(ppAvailableNetworkList.ToInt64()
						+ (uintSize * 2) /* Offset for dwNumberOfItems and dwIndex */
						+ (Marshal.SizeOf<WLAN_AVAILABLE_NETWORK>() * i) /* Offset for preceding items */);

					Network[i] = Marshal.PtrToStructure<WLAN_AVAILABLE_NETWORK>(availableNetwork);
				}
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WLAN_BSS_ENTRY
		{
			public DOT11_SSID dot11Ssid;
			public uint uPhyId;
			public DOT11_MAC_ADDRESS dot11Bssid;
			public DOT11_BSS_TYPE dot11BssType;
			public DOT11_PHY_TYPE dot11BssPhyType;
			public int lRssi;
			public uint uLinkQuality;

			[MarshalAs(UnmanagedType.U1)]
			public bool bInRegDomain;

			public ushort usBeaconPeriod;
			public ulong ullTimestamp;
			public ulong ullHostTimestamp;
			public ushort usCapabilityInformation;
			public uint ulChCenterFrequency;
			public WLAN_RATE_SET wlanRateSet;
			public uint ulIeOffset;
			public uint ulIeSize;
		}

		public struct WLAN_BSS_LIST
		{
			public uint dwTotalSize;
			public uint dwNumberOfItems;
			public WLAN_BSS_ENTRY[] wlanBssEntries;

			public WLAN_BSS_LIST(IntPtr ppWlanBssList)
			{
				var uintSize = Marshal.SizeOf<uint>(); // 4

				dwTotalSize = (uint)Marshal.ReadInt32(ppWlanBssList, 0);
				dwNumberOfItems = (uint)Marshal.ReadInt32(ppWlanBssList, uintSize /* Offset for dwTotalSize */);
				wlanBssEntries = new WLAN_BSS_ENTRY[dwNumberOfItems];

				for (int i = 0; i < dwNumberOfItems; i++)
				{
					var wlanBssEntry = new IntPtr(ppWlanBssList.ToInt64()
						+ (uintSize * 2) /* Offset for dwTotalSize and dwNumberOfItems */
						+ (Marshal.SizeOf<WLAN_BSS_ENTRY>() * i) /* Offset for preceding items */);

					wlanBssEntries[i] = Marshal.PtrToStructure<WLAN_BSS_ENTRY>(wlanBssEntry);
				}
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WLAN_PROFILE_INFO
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strProfileName;

			public uint dwFlags;
		}

		public struct WLAN_PROFILE_INFO_LIST
		{
			public uint dwNumberOfItems;
			public uint dwIndex;
			public WLAN_PROFILE_INFO[] ProfileInfo;

			public WLAN_PROFILE_INFO_LIST(IntPtr ppProfileList)
			{
				var uintSize = Marshal.SizeOf<uint>(); // 4

				dwNumberOfItems = (uint)Marshal.ReadInt32(ppProfileList, 0);
				dwIndex = (uint)Marshal.ReadInt32(ppProfileList, uintSize /* Offset for dwNumberOfItems */);
				ProfileInfo = new WLAN_PROFILE_INFO[dwNumberOfItems];

				for (int i = 0; i < dwNumberOfItems; i++)
				{
					var profileInfo = new IntPtr(ppProfileList.ToInt64()
						+ (uintSize * 2) /* Offset for dwNumberOfItems and dwIndex */
						+ (Marshal.SizeOf<WLAN_PROFILE_INFO>() * i) /* Offset for preceding items */);

					ProfileInfo[i] = Marshal.PtrToStructure<WLAN_PROFILE_INFO>(profileInfo);
				}
			}
		}

		#endregion

		#region Struct (Secondary)

		[StructLayout(LayoutKind.Sequential)]
		public struct DOT11_SSID
		{
			public uint uSSIDLength;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] ucSSID;

			/// <summary>
			/// Returns the byte array of SSID.
			/// </summary>
			/// <returns>Byte array</returns>
			public byte[] ToBytes() => ucSSID?.Take((int)uSSIDLength).ToArray();

			private static Lazy<Encoding> _encoding = new Lazy<Encoding>(() =>
				Encoding.GetEncoding(65001, // UTF-8 code page
					EncoderFallback.ReplacementFallback,
					DecoderFallback.ExceptionFallback));

			/// <summary>
			/// Returns the UTF-8 string representation of SSID
			/// </summary>
			/// <returns>UTF-8 string if successfully converted the byte array of SSID. Null if failed.</returns>
			public override string ToString()
			{
				if (ucSSID != null)
				{
					try
					{
						return _encoding.Value.GetString(ToBytes());
					}
					catch (DecoderFallbackException)
					{ }
				}
				return null;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct DOT11_MAC_ADDRESS
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] ucDot11MacAddress;

			/// <summary>
			/// Returns the byte array of MAC address
			/// </summary>
			/// <returns></returns>
			public byte[] ToBytes() => ucDot11MacAddress?.ToArray();

			/// <summary>
			/// Returns the hexadecimal string representation of MAC address delimited by colon.
			/// </summary>
			/// <returns>Hexadecimal string</returns>
			public override string ToString()
			{
				return (ucDot11MacAddress != null)
					? BitConverter.ToString(ucDot11MacAddress).Replace('-', ':')
					: null;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WLAN_ASSOCIATION_ATTRIBUTES
		{
			public DOT11_SSID dot11Ssid;
			public DOT11_BSS_TYPE dot11BssType;
			public DOT11_MAC_ADDRESS dot11Bssid;
			public DOT11_PHY_TYPE dot11PhyType;
			public uint uDot11PhyIndex;
			public uint wlanSignalQuality;
			public uint ulRxRate;
			public uint ulTxRate;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WLAN_SECURITY_ATTRIBUTES
		{
			[MarshalAs(UnmanagedType.Bool)]
			public bool bSecurityEnabled;

			[MarshalAs(UnmanagedType.Bool)]
			public bool bOneXEnabled;

			public DOT11_AUTH_ALGORITHM dot11AuthAlgorithm;
			public DOT11_CIPHER_ALGORITHM dot11CipherAlgorithm;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WLAN_RATE_SET
		{
			public uint uRateSetLength;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 126)]
			public ushort[] usRateSet;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WLAN_CONNECTION_PARAMETERS
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
			public DOT11_MAC_ADDRESS BSSIDs;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NDIS_OBJECT_HEADER
		{
			public byte Type;
			public byte Revision;
			public ushort Size;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WLAN_NOTIFICATION_DATA
		{
			public uint NotificationSource;
			public uint NotificationCode;
			public Guid InterfaceGuid;
			public uint dwDataSize;
			public IntPtr pData;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WLAN_CONNECTION_NOTIFICATION_DATA
		{
			public WLAN_CONNECTION_MODE wlanConnectionMode;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strProfileName;

			public DOT11_SSID dot11Ssid;
			public DOT11_BSS_TYPE dot11BssType;

			[MarshalAs(UnmanagedType.Bool)]
			public bool bSecurityEnabled;

			public uint wlanReasonCode; // WLAN_REASON_CODE
			public uint dwFlags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
			public string strProfileXml;
		}

		#endregion

		#region Enum

		public enum WLAN_INTERFACE_STATE
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

		public enum WLAN_CONNECTION_MODE
		{
			wlan_connection_mode_profile,
			wlan_connection_mode_temporary_profile,
			wlan_connection_mode_discovery_secure,
			wlan_connection_mode_discovery_unsecure,
			wlan_connection_mode_auto,
			wlan_connection_mode_invalid
		}

		public enum WLAN_INTERFACE_TYPE
		{
			wlan_interface_type_emulated_802_11 = 0,
			wlan_interface_type_native_802_11,
			wlan_interface_type_invalid
		}

		public enum DOT11_RADIO_STATE
		{
			dot11_radio_state_unknown,
			dot11_radio_state_on,
			dot11_radio_state_off
		}

		public enum DOT11_BSS_TYPE
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

		public enum DOT11_PHY_TYPE : uint
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

		public enum DOT11_AUTH_ALGORITHM : uint
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

		public enum DOT11_CIPHER_ALGORITHM : uint
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

		public enum WLAN_INTF_OPCODE : uint
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

		public enum WLAN_NOTIFICATION_ACM : uint
		{
			wlan_notification_acm_start = 0,
			wlan_notification_acm_autoconf_enabled,
			wlan_notification_acm_autoconf_disabled,
			wlan_notification_acm_background_scan_enabled,
			wlan_notification_acm_background_scan_disabled,
			wlan_notification_acm_bss_type_change,
			wlan_notification_acm_power_setting_change,
			wlan_notification_acm_scan_complete,
			wlan_notification_acm_scan_fail,
			wlan_notification_acm_connection_start,
			wlan_notification_acm_connection_complete,
			wlan_notification_acm_connection_attempt_fail,
			wlan_notification_acm_filter_list_change,
			wlan_notification_acm_interface_arrival,
			wlan_notification_acm_interface_removal,
			wlan_notification_acm_profile_change,
			wlan_notification_acm_profile_name_change,
			wlan_notification_acm_profiles_exhausted,
			wlan_notification_acm_network_not_available,
			wlan_notification_acm_network_available,
			wlan_notification_acm_disconnecting,
			wlan_notification_acm_disconnected,
			wlan_notification_acm_adhoc_network_state_change,
			wlan_notification_acm_profile_unblocked,
			wlan_notification_acm_screen_power_change,
			wlan_notification_acm_profile_blocked,
			wlan_notification_acm_scan_list_refresh,
			wlan_notification_acm_end
		}

		#endregion

		public const uint WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_ADHOC_PROFILES = 0x00000001;
		public const uint WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_MANUAL_HIDDEN_PROFILES = 0x00000002;

		public const uint ERROR_SUCCESS = 0;
		public const uint ERROR_INVALID_PARAMETER = 87;
		public const uint ERROR_INVALID_HANDLE = 6;
		public const uint ERROR_INVALID_STATE = 5023;
		public const uint ERROR_NOT_FOUND = 1168;
		public const uint ERROR_NOT_ENOUGH_MEMORY = 8;
		public const uint ERROR_ACCESS_DENIED = 5;
		public const uint ERROR_NOT_SUPPORTED = 50;
		public const uint ERROR_SERVICE_NOT_ACTIVE = 1062;
		public const uint ERROR_NDIS_DOT11_AUTO_CONFIG_ENABLED = 0x80342000;
		public const uint ERROR_NDIS_DOT11_MEDIA_IN_USE = 0x80342001;
		public const uint ERROR_NDIS_DOT11_POWER_STATE_INVALID = 0x80342002;
		public const uint ERROR_ALREADY_EXISTS = 183;
		public const uint ERROR_BAD_PROFILE = 1206;
		public const uint ERROR_NO_MATCH = 1169;
		public const uint ERROR_GEN_FAILURE = 31;

		public const uint WLAN_NOTIFICATION_SOURCE_NONE = 0;
		public const uint WLAN_NOTIFICATION_SOURCE_ALL = 0x0000FFFF;
		public const uint WLAN_NOTIFICATION_SOURCE_ACM = 0x00000008;
		public const uint WLAN_NOTIFICATION_SOURCE_HNWK = 0x00000080;
		public const uint WLAN_NOTIFICATION_SOURCE_ONEX = 0x00000004;
		public const uint WLAN_NOTIFICATION_SOURCE_MSM = 0x00000010;
		public const uint WLAN_NOTIFICATION_SOURCE_SECURITY = 0x00000020;
		public const uint WLAN_NOTIFICATION_SOURCE_IHV = 0x00000040;

		public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
	}
}