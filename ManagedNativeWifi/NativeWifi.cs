using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
            [MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
            uint dwFlags,
            IntPtr pReserved,
            out IntPtr ppAvailableNetworkList);

        [DllImport("Wlanapi.dll", SetLastError = true)]
        private static extern uint WlanQueryInterface(
            IntPtr hClientHandle,
            [MarshalAs(UnmanagedType.LPStruct)] Guid interfaceGuid,
            WLAN_INTF_OPCODE OpCode,
            IntPtr pReserved,
            out uint pdwDataSize,
            ref IntPtr ppData,
            IntPtr pWlanOpcodeValueType);

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
                dwNumberOfItems = (uint)Marshal.ReadInt32(ppInterfaceList, 0); // 4 bytes
                dwIndex = (uint)Marshal.ReadInt32(ppInterfaceList, 4); // 4 bytes
                InterfaceInfo = new WLAN_INTERFACE_INFO[dwNumberOfItems];

                const int offset = 8; // 8 is bytes for dwNumberOfItems and dwIndex.

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
                dwNumberOfItems = (uint)Marshal.ReadInt32(ppAvailableNetworkList, 0); // 4 bytes
                dwIndex = (uint)Marshal.ReadInt32(ppAvailableNetworkList, 4); // 4 bytes
                Network = new WLAN_AVAILABLE_NETWORK[dwNumberOfItems];

                const int offset = 8; // 8 is bytes for dwNumberOfItems and dwIndex.

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

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string ucSSID;

            public string ToSsidString()
            {
                if ((ucSSID == null) || (ucSSID.Length < (int)uSSIDLength))
                    return null;

                return ucSSID.Substring(0, (int)uSSIDLength);
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

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
            public string dot11Bssid; // DOT11_MAC_ADDRESS

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
            dot11_BSS_type_infrastructure = 1,
            dot11_BSS_type_independent = 2,
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

        private const uint ERROR_SUCCESS = 0U;
        private const uint ERROR_INVALID_STATE = 5023U;

        #endregion


        /// <summary>
        /// Get SSIDs of available WiFi networks.
        /// </summary>
        /// <returns>SSIDs</returns>
        public static IEnumerable<string> GetAvailableNetworkSsid()
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

                Debug.WriteLine("Interface info count: {0}", interfaceInfoList.dwNumberOfItems);

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
        /// Get SSIDs of connected WiFi networks.
        /// </summary>
        /// <returns>SSIDs</returns>
        public static IEnumerable<string> GetConnectedNetworkSsid()
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

                Debug.WriteLine("Interface info count: {0}", interfaceInfoList.dwNumberOfItems);

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
                        IntPtr.Zero) != ERROR_SUCCESS)
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
    }
}