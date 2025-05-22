using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace ManagedNativeWifi.Win32;

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

	[DllImport("Wlanapi.dll", SetLastError = true)]
	public static extern IntPtr WlanAllocateMemory(uint dwMemorySize);

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
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		WLAN_INTF_OPCODE OpCode,
		IntPtr pReserved,
		out uint pdwDataSize,
		out IntPtr ppData, // Pointer to queried data
		IntPtr pWlanOpcodeValueType);

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanGetInterfaceCapability(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		IntPtr pReserved,
		out IntPtr ppCapability); // Pointer to WLAN_INTERFACE_CAPABILITY

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanSetInterface(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		WLAN_INTF_OPCODE OpCode,
		uint dwDataSize,
		IntPtr pData, // Pointer to data to be set
		IntPtr pReserved);

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanScan(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		IntPtr pDot11Ssid,
		IntPtr pIeData,
		IntPtr pReserved);

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanGetAvailableNetworkList(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		uint dwFlags,
		IntPtr pReserved,
		out IntPtr ppAvailableNetworkList); // Pointer to WLAN_AVAILABLE_NETWORK_LIST

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanGetNetworkBssList(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		IntPtr pDot11Ssid,
		DOT11_BSS_TYPE dot11BssType,
		[MarshalAs(UnmanagedType.Bool)] bool bSecurityEnabled,
		IntPtr pReserved,
		out IntPtr ppWlanBssList); // Pointer to WLAN_BSS_LIST

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanGetProfileList(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		IntPtr pReserved,
		out IntPtr ppProfileList); // Pointer to WLAN_PROFILE_INFO_LIST

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanGetProfile(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		[MarshalAs(UnmanagedType.LPWStr)] string strProfileName,
		IntPtr pReserved,
		[MarshalAs(UnmanagedType.LPWStr)] out string pstrProfileXml,
		ref uint pdwFlags,
		out uint pdwGrantedAccess);

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanSetProfile(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		uint dwFlags,
		[MarshalAs(UnmanagedType.LPWStr)] string strProfileXml,
		[MarshalAs(UnmanagedType.LPWStr)] string strAllUserProfileSecurity,
		[MarshalAs(UnmanagedType.Bool)] bool bOverwrite,
		IntPtr pReserved,
		out uint pdwReasonCode); // WLAN_REASON_CODE

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanSetProfileEapXmlUserData(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		[MarshalAs(UnmanagedType.LPWStr)] string strProfileName,
		uint dwFlags,
		[MarshalAs(UnmanagedType.LPWStr)] string strEapXmlUserData,
		IntPtr pReserved);

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanSetProfilePosition(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		[MarshalAs(UnmanagedType.LPWStr)] string strProfileName,
		uint dwPosition,
		IntPtr pReserved);

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanRenameProfile(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		[MarshalAs(UnmanagedType.LPWStr)] string strOldProfileName,
		[MarshalAs(UnmanagedType.LPWStr)] string strNewProfileName,
		IntPtr pReserved);

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanDeleteProfile(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		[MarshalAs(UnmanagedType.LPWStr)] string strProfileName,
		IntPtr pReserved);

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanConnect(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		[In] ref WLAN_CONNECTION_PARAMETERS pConnectionParameters,
		IntPtr pReserved);

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanDisconnect(
		SafeClientHandle hClientHandle,
		[MarshalAs(UnmanagedType.LPStruct), In] Guid pInterfaceGuid,
		IntPtr pReserved);

	[DllImport("Wlanapi.dll")]
	public static extern uint WlanRegisterNotification(
		SafeClientHandle hClientHandle,
		WLAN_NOTIFICATION_SOURCE dwNotifSource,
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

		private static readonly Lazy<Encoding> _encoding = new Lazy<Encoding>(() =>
			Encoding.GetEncoding(65001, // UTF-8 code page
				EncoderFallback.ReplacementFallback,
				DecoderFallback.ExceptionFallback));

		/// <summary>
		/// Returns the string representation of SSID decoded by UTF-8.
		/// </summary>
		/// <returns>String representation of SSID if successfully decoded by UTF-8 from the byte array of SSID. Null if failed.</returns>
		public override string ToString()
		{
			if (ucSSID is not null)
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

		internal static bool TryCreate(byte[] rawBytes, out DOT11_SSID ssid)
		{
			if (rawBytes is not { Length: > 0 and <= 32 })
			{
				ssid = default;
				return false;
			}

			ssid = new DOT11_SSID
			{
				uSSIDLength = (uint)rawBytes.Length,
				ucSSID = new byte[32] // Array filled with 0
			};
			Buffer.BlockCopy(rawBytes, 0, ssid.ucSSID, 0, rawBytes.Length);
			return true;
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
			return (ucDot11MacAddress is not null)
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
		public WLAN_NOTIFICATION_SOURCE NotificationSource;
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

	[StructLayout(LayoutKind.Sequential)]
	public struct WLAN_REALTIME_CONNECTION_QUALITY_LINK_INFO
	{
		public byte ucLinkID;
		public uint ulChannelCenterFrequencyMhz;
		public uint ulBandwidth;
		public int lRssi;
		public WLAN_RATE_SET wlanRateSet;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WLAN_REALTIME_CONNECTION_QUALITY
	{
		public DOT11_PHY_TYPE dot11PhyType;
		public uint ulLinkQuality;
		public uint ulRxRate;
		public uint ulTxRate;
		[MarshalAs(UnmanagedType.Bool)]
		public bool bIsMLOConnection;
		public uint ulNumLinks;
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

		// Supplemented by description at DOT11_PHY_TYPE enumeration (windot11.h)
		dot11_phy_type_dmg = 9,
		dot11_phy_type_he = 10,
		dot11_phy_type_eht = 11,

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

		// Derived from wlantypes.h of Windows SDK (10.0.26100.0)
		DOT11_AUTH_ALGO_WPA3 = 8,
		DOT11_AUTH_ALGO_WPA3_ENT_192 = DOT11_AUTH_ALGO_WPA3,
		DOT11_AUTH_ALGO_WPA3_SAE = 9,
		DOT11_AUTH_ALGO_OWE = 10,
		DOT11_AUTH_ALGO_WPA3_ENT = 11,

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
		wlan_intf_opcode_secondary_sta_interfaces,
		wlan_intf_opcode_secondary_sta_synchronized_connections,
		wlan_intf_opcode_realtime_connection_quality,
		wlan_intf_opcode_qos_info,
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

	[Flags]
	public enum WLAN_NOTIFICATION_SOURCE : uint
	{
		WLAN_NOTIFICATION_SOURCE_NONE = 0,
		WLAN_NOTIFICATION_SOURCE_ONEX = 0x00000004,
		WLAN_NOTIFICATION_SOURCE_ACM = 0x00000008,
		WLAN_NOTIFICATION_SOURCE_MSM = 0x00000010,
		WLAN_NOTIFICATION_SOURCE_SECURITY = 0x00000020,
		WLAN_NOTIFICATION_SOURCE_IHV = 0x00000040,
		WLAN_NOTIFICATION_SOURCE_HNWK = 0x00000080,
		WLAN_NOTIFICATION_SOURCE_ALL = 0x0000FFFF
	}

	/// <summary>
	/// WLAN_NOTIFICATION_ACM enumeration:
	/// https://learn.microsoft.com/en-us/windows/win32/api/wlanapi/ne-wlanapi-wlan_notification_acm-r1
	/// </summary>
	/// <remarks>
	/// The descriptions are given at WLAN_NOTIFICATION_DATA structure:
	/// https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms706902(v=vs.85) 
	/// </remarks>
	public enum WLAN_NOTIFICATION_ACM : uint
	{
		wlan_notification_acm_start = 0,

		/// <summary>
		/// <para>Autoconfiguration is enabled.</para>
		/// </summary>
		wlan_notification_acm_autoconf_enabled,

		/// <summary>
		/// <para>Autoconfiguration is disabled.</para>
		/// </summary>
		wlan_notification_acm_autoconf_disabled,

		/// <summary>
		/// <para>Background scans are enabled.</para>
		/// </summary>
		wlan_notification_acm_background_scan_enabled,

		/// <summary>
		/// <para>Background scans are disabled.</para>
		/// </summary>
		wlan_notification_acm_background_scan_disabled,

		/// <summary>
		/// <para>The BSS type for an interface has changed.</para>
		/// <para>The pData member points to a DOT11_BSS_TYPE enumeration value that identifies
		/// the new basic service set (BSS) type.</para>
		/// </summary>
		wlan_notification_acm_bss_type_change,

		/// <summary>
		/// <para>The power setting for an interface has changed.</para>
		/// <para>The pData member points to a WLAN_POWER_SETTING enumeration value that
		/// identifies the new power setting of an interface.</para>
		/// </summary>
		wlan_notification_acm_power_setting_change,

		/// <summary>
		/// <para>A scan for networks has completed.</para>
		/// </summary>
		wlan_notification_acm_scan_complete,

		/// <summary>
		/// <para>A scan for connectable networks failed.</para>
		/// <para>The pData member points to a WLAN_REASON_CODE data type value that identifies
		/// the reason the WLAN operation failed.</para>
		/// </summary>
		wlan_notification_acm_scan_fail,

		/// <summary>
		/// <para>A connection has started to a network in range.</para>
		/// <para>The pData member points to a WLAN_CONNECTION_NOTIFICATION_DATA structure that
		/// identifies the network information for the connection attempt.</para>
		/// </summary>
		wlan_notification_acm_connection_start,

		/// <summary>
		/// <para>A connection has completed.</para>
		/// <para>The pData member points to a WLAN_CONNECTION_NOTIFICATION_DATA structure that
		/// identifies the network information for the connection attempt that completed.
		/// The connection succeeded if the wlanReasonCode in WLAN_CONNECTION_NOTIFICATION_DATA
		/// is WLAN_REASON_CODE_SUCCESS. Otherwise, the connection has failed.</para>
		/// </summary>
		wlan_notification_acm_connection_complete,

		/// <summary>
		/// <para>A connection attempt has failed.</para>
		/// <para>A connection consists of one or more connection attempts. An application may
		/// receive zero or more wlan_notification_acm_connection_attempt_fail notifications
		/// between receiving the wlan_notification_acm_connection_start notification and
		/// the wlan_notification_acm_connection_complete notification.</para>
		/// <para>The pData member points to a WLAN_CONNECTION_NOTIFICATION_DATA structure that
		/// identifies the network information for the connection attempt that failed.</para>
		/// </summary>
		wlan_notification_acm_connection_attempt_fail,

		/// <summary>
		/// <para>A change in the filter list has occurred, either through group policy or
		/// a call to the WlanSetFilterList function.</para>
		/// <para>An application can call the WlanGetFilterList function to retrieve the new
		/// filter list.</para>
		/// </summary>
		wlan_notification_acm_filter_list_change,

		/// <summary>
		/// <para>A wireless LAN interface has been added to or enabled on the local computer.</para>
		/// </summary>
		wlan_notification_acm_interface_arrival,

		/// <summary>
		/// <para>A wireless LAN interface has been removed or disabled on the local computer.</para>
		/// </summary>
		wlan_notification_acm_interface_removal,

		/// <summary>
		/// <para>A change in a profile or the profile list has occurred, either through group
		/// policy or by calls to Native Wifi functions.</para>
		/// <para>An application can call the WlanGetProfileList and WlanGetProfile functions
		/// to retrieve the updated profiles. The interface on which the profile list changes
		/// is identified by the InterfaceGuid member.</para>
		/// </summary>
		wlan_notification_acm_profile_change,

		/// <summary>
		/// <para>A profile name has changed, either through group policy or by calls to Native
		/// Wifi functions.</para>
		/// <para>The pData member points to a buffer that contains two NULL-terminated WCHAR
		/// strings, the old profile name followed by the new profile name.</para>
		/// </summary>
		wlan_notification_acm_profile_name_change,

		/// <summary>
		/// <para>All profiles were exhausted in an attempt to autoconnect.</para>
		/// </summary>
		wlan_notification_acm_profiles_exhausted,

		/// <summary>
		/// <para>The wireless service cannot find any connectable network after a scan.</para>
		/// <para>The interface on which no connectable network is found is identified by
		/// the InterfaceGuid member.</para>
		/// </summary>
		wlan_notification_acm_network_not_available,

		/// <summary>
		/// <para>The wireless service found a connectable network after a scan, the interface
		/// was in the disconnected state, and there is no compatible auto-connect profile that
		/// the wireless service can use to connect.</para>
		/// <para>The interface on which connectable networks are found is identified by
		/// the InterfaceGuid member.</para>
		/// </summary>
		wlan_notification_acm_network_available,

		/// <summary>
		/// <para>The wireless service is disconnecting from a connectable network.</para>
		/// <para>The pData member points to a WLAN_CONNECTION_NOTIFICATION_DATA structure that
		/// identifies the network information for the connection that is disconnecting.</para>
		/// </summary>
		wlan_notification_acm_disconnecting,

		/// <summary>
		/// <para>The wireless service has disconnected from a connectable network.</para>
		/// <para>The pData member points to a WLAN_CONNECTION_NOTIFICATION_DATA structure that
		/// identifies the network information for the connection that disconnected.</para>
		/// </summary>
		wlan_notification_acm_disconnected,

		/// <summary>
		/// <para>A state change has occurred for an adhoc network.</para>
		/// <para>The pData member points to a WLAN_ADHOC_NETWORK_STATE enumeration value that
		/// identifies the new adhoc network state.</para>
		/// </summary>
		wlan_notification_acm_adhoc_network_state_change,

		wlan_notification_acm_profile_unblocked,

		/// <summary>
		/// <para>The screen power has changed.</para>
		/// <para>The pData member points to a BOOL value that indicates the value of
		/// the screen power change. When this value is TRUE, the screen changed to on.
		/// When this value is FALSE, the screen changed to off.</para>
		/// </summary>
		wlan_notification_acm_screen_power_change,

		wlan_notification_acm_profile_blocked,
		wlan_notification_acm_scan_list_refresh,
		wlan_notification_acm_end
	}

	/// <summary>
	/// WLAN_NOTIFICATION_MSM enumeration:
	/// https://learn.microsoft.com/en-us/windows/win32/api/wlanapi/ne-wlanapi-wlan_notification_msm-r1
	/// </summary>
	/// <remarks>
	/// The descriptions are given at WLAN_NOTIFICATION_DATA structure:
	/// https://learn.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms706902(v=vs.85)
	/// </remarks>
	public enum WLAN_NOTIFICATION_MSM : uint
	{
		wlan_notification_msm_start = 0,

		/// <summary>
		/// <para>A wireless device is in the process of associating with an access point or
		/// a peer station.</para>
		/// <para>The pData member points to a WLAN_MSM_NOTIFICATION_DATA structure that contains
		/// connection-related information</para>
		/// </summary>
		wlan_notification_msm_associating,

		/// <summary>
		/// <para>The wireless device has associated with an access point or a peer station.</para>
		/// <para>The pData member points to a WLAN_MSM_NOTIFICATION_DATA structure that contains
		/// connection-related information.</para>
		/// </summary>
		wlan_notification_msm_associated,

		/// <summary>
		/// <para>The wireless device is in the process of authenticating.</para>
		/// <para>The pData member of the WLAN_NOTIFICATION_DATA structure points to
		/// a WLAN_MSM_NOTIFICATION_DATA structure that contains connection-related information.</para>
		/// </summary>
		wlan_notification_msm_authenticating,

		/// <summary>
		/// <para>The wireless device is associated with an access point or a peer station, keys
		/// have been exchanged, and the wireless device is available to send data.</para>
		/// <para>The pData member points to a WLAN_MSM_NOTIFICATION_DATA structure that contains
		/// connection-related information.</para>
		/// </summary>
		wlan_notification_msm_connected,

		/// <summary>
		/// <para>The wireless device is connected to an access point and has initiated roaming to
		/// another access point.</para>
		/// <para>The pData member points to a WLAN_MSM_NOTIFICATION_DATA structure that contains
		/// connection-related information.</para>
		/// </summary>
		wlan_notification_msm_roaming_start,

		/// <summary>
		/// <para>The wireless device was connected to an access point and has completed roaming
		/// to another access point.</para>
		/// <para>The pData member points to a WLAN_MSM_NOTIFICATION_DATA structure that contains
		/// connection-related information.</para>
		/// </summary>
		wlan_notification_msm_roaming_end,

		/// <summary>
		/// <para>The radio state for an adapter has changed. Each physical layer (PHY) has its
		/// own radio state. The radio for an adapter is switched off when the radio state of
		/// every PHY is off.</para>
		/// <para>The pData member points to a WLAN_PHY_RADIO_STATE structure that identifies
		/// the new radio state.</para>
		/// </summary>
		wlan_notification_msm_radio_state_change,

		/// <summary>
		/// <para>A signal quality change for the currently associated access point or peer
		/// station.</para>
		/// <para>The pData member points to a ULONG value for the WLAN_SIGNAL_QUALITY that
		/// identifies the new signal quality.</para>
		/// </summary>
		wlan_notification_msm_signal_quality_change,

		/// <summary>
		/// <para>A wireless device is in the process of disassociating from an access point or
		/// a peer station.</para>
		/// <para>The pData member points to a WLAN_MSM_NOTIFICATION_DATA structure that contains
		/// connection-related information.</para>
		/// </summary>
		wlan_notification_msm_disassociating,

		/// <summary>
		/// <para>The wireless device is not associated with an access point or a peer station.</para>
		/// <para>The pData member points to a WLAN_MSM_NOTIFICATION_DATA structure that contains
		/// connection-related information. The wlanReasonCode member of
		/// the WLAN_MSM_NOTIFICATION_DATA structure indicates the reason for the disconnect.</para>
		/// </summary>
		wlan_notification_msm_disconnected,

		/// <summary>
		/// <para>A peer has joined an adhoc network.</para>
		/// <para>The pData member points to a WLAN_MSM_NOTIFICATION_DATA structure that contains
		/// connection-related information.</para>
		/// </summary>
		wlan_notification_msm_peer_join,

		/// <summary>
		/// <para>A peer has left an adhoc network.</para>
		/// <para>The pData member of the WLAN_NOTIFICATION_DATA structure points to
		/// a WLAN_MSM_NOTIFICATION_DATA structure that contains connection-related information.</para>
		/// </summary>
		wlan_notification_msm_peer_leave,

		/// <summary>
		/// <para>A wireless adapter has been removed from the local computer.</para>
		/// <para>The pData member points to a WLAN_MSM_NOTIFICATION_DATA structure that contains
		/// connection-related information.</para>
		/// </summary>
		wlan_notification_msm_adapter_removal,

		/// <summary>
		/// <para>The operation mode of the wireless device has changed.</para>
		/// <para>The pData member points to a ULONG that identifies the new operation mode.</para>
		/// </summary>
		wlan_notification_msm_adapter_operation_mode_change,

		/// <summary>
		/// <para>The current link quality has degraded, but the system has not yet disconnected
		/// or reconnected.</para>
		/// </summary>
		wlan_notification_msm_link_degraded,

		/// <summary>
		/// <para>The link quality of the current Wi-Fi connection has improved after a previous
		/// degradation, without a disconnection.</para>
		/// </summary>
		wlan_notification_msm_link_improved,

		wlan_notification_msm_end
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
	public const uint ERROR_BUSY = 170;
	public const uint ERROR_GEN_FAILURE = 31;

	public const uint WLAN_REASON_CODE_SUCCESS = 0;

	public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
}