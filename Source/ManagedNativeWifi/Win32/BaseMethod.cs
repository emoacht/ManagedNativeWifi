using ManagedNativeWifi.Win32.Structures;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.NetworkManagement.Ndis;
using Windows.Win32.NetworkManagement.WiFi;
// ReSharper disable InconsistentNaming
// ReSharper disable GrammarMistakeInComment


namespace ManagedNativeWifi.Win32
{
	internal static partial class BaseMethod
	{
		private const uint WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_MANUAL_HIDDEN_PROFILES = 0x00000002;

		public static unsafe WLAN_INTERFACE_INFO[] GetInterfaceInfoList(SafeClientHandle clientHandle)
		{
			WLAN_INTERFACE_INFO_LIST* interfaceList = null;
			try
			{
				var result = WinApi.WlanEnumInterfaces(
					clientHandle,
					out interfaceList);

				// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
				// ERROR_INVALID_PARAMETER: A parameter is incorrect.
				// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request.
				CheckResult(nameof(WinApi.WlanEnumInterfaces), result, true);

				var items = interfaceList->dwNumberOfItems;
				var data = new WLAN_INTERFACE_INFO[items];

				for (var i = 0; i < items; i++)
				{
					data[i] = interfaceList->InterfaceInfo[i];
				}

				return data;
			}
			finally
			{
				if (interfaceList != null)
					WinApi.WlanFreeMemory(interfaceList);
			}
		}

		public static unsafe bool Scan(SafeClientHandle clientHandle, Guid interfaceId, DOT11_SSID? ssid)
		{
			var result = WinApi.WlanScan(
				clientHandle,
				in interfaceId,
				ssid,
				null);

			// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_NOT_ENOUGH_MEMORY: Failed to allocate memory for the query results.
			// ERROR_NDIS_DOT11_POWER_STATE_INVALID: The interface is turned off.
			// ERROR_BUSY: The requested resource is in use.
			return CheckResult(nameof(WinApi.WlanScan), result, false);
		}

		public static unsafe (ActionResult result, WLAN_AVAILABLE_NETWORK[] list) GetAvailableNetworkList(
			SafeClientHandle clientHandle, Guid interfaceId)
		{
			WLAN_AVAILABLE_NETWORK_LIST* availableNetworkList = null;
			try
			{
				var result = WinApi.WlanGetAvailableNetworkList(
					clientHandle,
					in interfaceId,
					WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_MANUAL_HIDDEN_PROFILES,
					out availableNetworkList);

				// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
				// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
				// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request.
				// ERROR_NDIS_DOT11_POWER_STATE_INVALID: The interface is turned off.
				// ERROR_NOT_FOUND: The specified interface is not found.
				CheckResult(nameof(WinApi.WlanGetAvailableNetworkList), result, false);

				switch ((WIN32_ERROR)result)
				{
					case WIN32_ERROR.ERROR_NOT_FOUND:
						return (ActionResult.NotFound, []);
					case WIN32_ERROR.ERROR_SUCCESS:
					{
						var items = availableNetworkList->dwNumberOfItems;
						var data = new WLAN_AVAILABLE_NETWORK[items];
						for (var i = 0; i < items; i++)
						{
							data[i] = availableNetworkList->Network[i];
						}

						return (ActionResult.Success, data);
					}
					default:
						return (ActionResult.OtherError, []);
				}
			}
			finally
			{
				if (availableNetworkList != null)
					WinApi.WlanFreeMemory(availableNetworkList);
			}
		}

		public static (ActionResult result, WLAN_BSS_ENTRY[] list) GetNetworkBssEntryList(SafeClientHandle clientHandle,
			Guid interfaceId)
		{
			return GetNetworkBssEntryList(
				clientHandle,
				interfaceId,
				null,
				DOT11_BSS_TYPE.dot11_BSS_type_any,
				false);
		}

		public static unsafe (ActionResult result, WLAN_BSS_ENTRY[] list) GetNetworkBssEntryList(
			SafeClientHandle clientHandle,
			Guid interfaceId, DOT11_SSID? ssid, DOT11_BSS_TYPE bssType, bool isSecurityEnabled)
		{
			WLAN_BSS_LIST* bssList = null;
			try
			{
				var result = WinApi.WlanGetNetworkBssList(
					clientHandle,
					in interfaceId,
					ssid,
					bssType,
					isSecurityEnabled,
					out bssList);

				// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
				// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
				// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request.
				// ERROR_NDIS_DOT11_POWER_STATE_INVALID: The interface is turned off.
				// ERROR_NOT_FOUND: The specified interface is not found.
				// ERROR_NOT_SUPPORTED: The WLAN AutoConfig service is disabled.
				// ERROR_SERVICE_NOT_ACTIVE: The WLAN AutoConfig service has not been started.
				CheckResult(nameof(WinApi.WlanGetNetworkBssList), result, false);

				switch ((WIN32_ERROR)result)
				{
					case WIN32_ERROR.ERROR_SUCCESS:
					{
						var items = bssList->dwNumberOfItems;
						var data = new WLAN_BSS_ENTRY[items];
						for (var i = 0; i < items; i++)
						{
							data[i] = bssList->wlanBssEntries[i];
						}

						return (ActionResult.Success, data);
					}
					case WIN32_ERROR.ERROR_NOT_FOUND:
						return (ActionResult.NotFound, []);
					default:
						return (ActionResult.OtherError, []);
				}
			}
			finally
			{
				if (bssList != null)
					WinApi.WlanFreeMemory(bssList);
			}
		}

		public static (ActionResult result, WLAN_CONNECTION_ATTRIBUTES value) GetCurrentConnection(
			SafeClientHandle clientHandle, Guid interfaceId)
		{
			// ERROR_INVALID_STATE: The interface is not connected to a network.
			// ERROR_NOT_FOUND: The specified interface is not found.
			return QueryInterface(
				clientHandle,
				interfaceId,
				WLAN_INTF_OPCODE.wlan_intf_opcode_current_connection,
				marshal: pData => pData == null
					? default
					: Marshal.PtrToStructure<WLAN_CONNECTION_ATTRIBUTES>((nint)pData));
		}

		public static (ActionResult result, int value) GetRssi(SafeClientHandle clientHandle, Guid interfaceId)
		{
			// ERROR_INVALID_STATE: The interface is not connected to a network.
			// ERROR_NOT_FOUND: The specified interface is not found.
			return QueryInterface(
				clientHandle,
				interfaceId,
				WLAN_INTF_OPCODE.wlan_intf_opcode_rssi,
				marshal: queryData => queryData == null
					? 0
					: Marshal.ReadInt32((nint)queryData));
		}

		public static (ActionResult result, WLAN_REALTIME_CONNECTION_QUALITY? value) GetRealtimeConnectionQuality(
			SafeClientHandle clientHandle, Guid interfaceId)
		{
			// ERROR_INVALID_STATE: The interface is not connected to a network.
			// ERROR_NOT_FOUND: The specified interface is not found.
			// ERROR_NOT_SUPPORTED: This query is not supported by the OS.
			return QueryInterface(
				clientHandle,
				interfaceId,
				WLAN_INTF_OPCODE.wlan_intf_opcode_realtime_connection_quality,
				marshal: queryData => queryData == null
					? default
					: Marshal.PtrToStructure<WLAN_REALTIME_CONNECTION_QUALITY>((nint)queryData));
		}

		public static unsafe WLAN_PROFILE_INFO[] GetProfileInfoList(SafeClientHandle clientHandle, Guid interfaceId)
		{
			WLAN_PROFILE_INFO_LIST* profileList = null;
			try
			{
				var result = WinApi.WlanGetProfileList(
					clientHandle,
					in interfaceId,
					out profileList);

				// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
				// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
				// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request.

				if (CheckResult(nameof(WinApi.WlanGetProfileList), result, false))
				{
					var items = profileList->dwNumberOfItems;
					var data = new WLAN_PROFILE_INFO[items];
					for (var i = 0; i < items; i++)
					{
						data[i] = profileList->ProfileInfo[i];
					}

					return data;
				}
				else
					return [];
			}
			finally
			{
				if (profileList != null)
					WinApi.WlanFreeMemory(profileList);
			}
		}

		public static string GetProfile(SafeClientHandle clientHandle, Guid interfaceId, string profileName,
			out uint profileTypeFlag)
		{
			var flags = 0U;
			var result = WinApi.WlanGetProfile(
				clientHandle,
				in interfaceId,
				profileName,
				out var pProfileXml,
				ref flags,
				out _);

			var profileXml = pProfileXml.ToString();
			profileTypeFlag = flags;

			// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_NOT_ENOUGH_MEMORY: Not enough storage is available to process this command.
			// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
			// ERROR_NOT_FOUND: The profile is not found.
			return CheckResult(nameof(WinApi.WlanGetProfile), result, false)
				? profileXml
				: string.Empty; // To be used
		}

		public static bool SetProfile(SafeClientHandle clientHandle, Guid interfaceId, uint profileTypeFlag,
			string profileXml, string profileSecurity, bool overwrite)
		{
			var result = WinApi.WlanSetProfile(
				clientHandle,
				in interfaceId,
				profileTypeFlag,
				profileXml,
				profileSecurity,
				overwrite,
				out var pdwReasonCode);

			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
			// ERROR_ALREADY_EXISTS: The profile already exists.
			// ERROR_BAD_PROFILE: The profile XML is not valid.
			// ERROR_NO_MATCH: The capabilities specified in the profile is not supported by the interface.
			return CheckResult(nameof(WinApi.WlanSetProfile), result, false, pdwReasonCode);
		}

		public static bool SetProfileEapXmlUserData(SafeClientHandle clientHandle, Guid interfaceId, string profileName,
			uint eapXmlFlag, string userDataXml)
		{
			var result = WinApi.WlanSetProfileEapXmlUserData(
				clientHandle,
				in interfaceId,
				profileName,
				(WLAN_SET_EAPHOST_FLAGS)eapXmlFlag,
				userDataXml);

			// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect.
			// ERROR_NOT_ENOUGH_MEMORY: Not enough storage is available to process this command.
			// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
			// ERROR_BAD_PROFILE: The EAP XML is not valid.
			// ERROR_NOT_SUPPORTED: The profile does not allow storage of user data.
			// ERROR_SERVICE_NOT_ACTIVE: The WLAN service has not been started.
			return CheckResult(nameof(WinApi.WlanSetProfileEapXmlUserData), result, false);
		}

		public static bool SetProfilePosition(SafeClientHandle clientHandle, Guid interfaceId, string profileName,
			uint position)
		{
			var result = WinApi.WlanSetProfilePosition(
				clientHandle,
				in interfaceId,
				profileName,
				position);

			// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
			// ERROR_NOT_FOUND: The position of a profile is invalid.
			return CheckResult(nameof(WinApi.WlanSetProfilePosition), result, false);
		}

		public static bool RenameProfile(SafeClientHandle clientHandle, Guid interfaceId, string oldProfileName,
			string newProfileName)
		{
			var result = WinApi.WlanRenameProfile(
				clientHandle,
				in interfaceId,
				oldProfileName,
				newProfileName);

			// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
			// ERROR_NOT_FOUND: The profile is not found in the profile store.
			return CheckResult(nameof(WinApi.WlanRenameProfile), result, false);
		}

		public static bool DeleteProfile(SafeClientHandle clientHandle, Guid interfaceId, string profileName)
		{
			var result = WinApi.WlanDeleteProfile(
				clientHandle,
				in interfaceId,
				profileName);

			// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
			// ERROR_NOT_FOUND: The profile is not found in the profile store.
			return CheckResult(nameof(WinApi.WlanDeleteProfile), result, false);
		}

		public static unsafe bool Connect(SafeClientHandle clientHandle, Guid interfaceId, string profileName,
			DOT11_BSS_TYPE bssType)
		{
			fixed(char* pProfileName = profileName)
			{
				var connectionParameters = new WLAN_CONNECTION_PARAMETERS
				{
					wlanConnectionMode = WLAN_CONNECTION_MODE.wlan_connection_mode_profile,
					strProfile = pProfileName,
					dot11BssType = bssType,
					dwFlags = 0U
				};

				var result = WinApi.WlanConnect(
					clientHandle,
					in interfaceId,
					in connectionParameters);

				// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
				// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
				// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
				// ERROR_NOT_FOUND: The interface is removed.
				return CheckResult(nameof(WinApi.WlanConnect), result, false);
			}
		}

		public static unsafe bool Connect(SafeClientHandle clientHandle, Guid interfaceId, string profileName,
			DOT11_BSS_TYPE bssType, DOT11_MAC_ADDRESS bssid)
		{
			var bssidListSize = (uint)(Marshal.SizeOf<DOT11_BSSID_LIST>() + Marshal.SizeOf(bssid) - 1);
			var pBssidList = (DOT11_BSSID_LIST*)WinApi.WlanAllocateMemory(bssidListSize);

			if (pBssidList == null)
				throw new Win32Exception("Failed to allocate memory for DOT11_BSSID_LIST.");
		
			try
			{
				fixed (char* pProfileName = profileName)
				{
					pBssidList->Header = new NDIS_OBJECT_HEADER
					{
						Type = 0x80, //NDIS_OBJECT_TYPE_DEFAULT
						Revision = 1, //DOT11_BSSID_LIST_REVISION_1
						Size = (ushort)bssidListSize
					};
					pBssidList->uNumOfEntries = 1;
					pBssidList->uTotalNumOfEntries = 1;
					if (bssid.ucDot11MacAddress?.Length > 0)
					{
						bssid.ucDot11MacAddress.AsSpan()
							.CopyTo(pBssidList->BSSIDs.AsSpan(bssid.ucDot11MacAddress.Length));
					}

					var connectionParameters = new WLAN_CONNECTION_PARAMETERS
					{
						wlanConnectionMode = WLAN_CONNECTION_MODE.wlan_connection_mode_profile,
						strProfile = pProfileName,
						dot11BssType = bssType,
						dwFlags = 0U,
						pDesiredBssidList = pBssidList
					};

					var result = WinApi.WlanConnect(
						clientHandle,
						in interfaceId,
						in connectionParameters);

					return CheckResult(nameof(WinApi.WlanConnect), result, false);
				}
			}
			finally
			{
				WinApi.WlanFreeMemory(pBssidList);
			}
		
		}

		public static bool Disconnect(SafeClientHandle clientHandle, Guid interfaceId)
		{
			var result = WinApi.WlanDisconnect(
				clientHandle,
				in interfaceId);

			// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_NOT_ENOUGH_MEMORY: Failed to allocate memory for the query results.
			// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
			// ERROR_NOT_FOUND: The interface is removed.
			return CheckResult(nameof(WinApi.WlanDisconnect), result, false);
		}

		public static unsafe WLAN_INTERFACE_CAPABILITY GetInterfaceCapability(SafeClientHandle clientHandle,
			Guid interfaceId)
		{
			WLAN_INTERFACE_CAPABILITY* pCapability = null;
			try
			{
				var result = WinApi.WlanGetInterfaceCapability(
					clientHandle,
					in interfaceId,
					out pCapability);

				// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
				// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
				return CheckResult(nameof(WinApi.WlanGetInterfaceCapability), result, false)
					? *pCapability
					: default;
			}
			finally
			{
				if (pCapability != null)
					WinApi.WlanFreeMemory(pCapability);
			}
		}

		public static unsafe WLAN_PHY_RADIO_STATE[] GetPhyRadioStates(SafeClientHandle clientHandle, Guid interfaceId)
		{
			var (result, value) = QueryInterface(
				clientHandle,
				interfaceId,
				WLAN_INTF_OPCODE.wlan_intf_opcode_radio_state,
				marshal: queryData =>
				{
					if (queryData == null)
						return [];

					var pList = (WLAN_RADIO_STATE*)queryData;
					var count = pList->dwNumberOfPhys;
					var states = new WLAN_PHY_RADIO_STATE[count];

#if NETSTANDARD2_0_OR_GREATER
					var phyRadioStateArray = pList->PhyRadioState.ToArray();
#elif NET8_0_OR_GREATER
					var phyRadioStateArray = pList->PhyRadioState.AsSpan();
#endif
					for (var i = 0; i < count; i++)
					{
						states[i] =phyRadioStateArray[i];
					}


					return states;
				});

			return result is ActionResult.Success
				? value ?? []
				: [];
		}

		public static bool SetPhyRadioState(SafeClientHandle clientHandle, Guid interfaceId, WLAN_PHY_RADIO_STATE state)
		{
			// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
			// By default, only a user who is logged on as a member of the Administrators group or
			// the Network Configuration Operators group can set the operation mode of the interface.
			// ERROR_GEN_FAILURE: The OpCode is not supported by the driver or NIC.
			var result = SetInterface(
				clientHandle,
				interfaceId,
				WLAN_INTF_OPCODE.wlan_intf_opcode_radio_state,
				marshal: (pBuffer, _, value) =>
				{
					if (pBuffer == null) return;

					Marshal.StructureToPtr(value, (nint)pBuffer, false);
				},
				state);

			return result is ActionResult.Success;
		}

		public static bool IsAutoConfig(SafeClientHandle clientHandle, Guid interfaceId)
		{
			var (result, value) = QueryInterface(
				clientHandle,
				interfaceId,
				WLAN_INTF_OPCODE.wlan_intf_opcode_autoconf_enabled,
				marshal: queryData => queryData == null
					? (int?)null
					: Marshal.ReadInt32(queryData.Value));

			return result is ActionResult.Success
			       && value is not 0; // True = other than 0. False = 0.
		}

		private static unsafe (ActionResult Success, TValue Value) QueryInterface<TValue>(SafeClientHandle clientHandle,
			Guid interfaceId, WLAN_INTF_OPCODE opCode, Func<HANDLE?, TValue> marshal)
		{
			var result = WinApi.WlanQueryInterface(
				clientHandle,
				interfaceId,
				opCode,
				out _,
				out var queryData);

			CheckResult(nameof(WinApi.WlanQueryInterface), result, false);

			if ((WIN32_ERROR)result == WIN32_ERROR.ERROR_SUCCESS)
			{
				try
				{
					return (ActionResult.Success, marshal.Invoke((HANDLE)queryData));
				}
				finally
				{
					if (queryData != null)
						WinApi.WlanFreeMemory(queryData);
				}
			}
		
			return (WIN32_ERROR)result switch{
				WIN32_ERROR.ERROR_INVALID_STATE => (ActionResult.NotConnected, default),
				WIN32_ERROR.ERROR_NOT_FOUND => (ActionResult.NotFound, default),
				WIN32_ERROR.ERROR_NOT_SUPPORTED => (ActionResult.NotSupported, default),
				_ => (ActionResult.OtherError, default)
			};
		}

		private static unsafe ActionResult SetInterface<TValue>(SafeClientHandle clientHandle, Guid interfaceId,
			WLAN_INTF_OPCODE opCode, Action<HANDLE?, int, TValue> marshal, TValue value)
		{
			var size = Marshal.SizeOf(value);
			var pBuffer = HANDLE.Null;
			try
			{
				pBuffer = (HANDLE)WinApi.WlanAllocateMemory((uint)size);
				marshal.Invoke(pBuffer, size, value);
				var buffer = new Span<byte>(pBuffer, size);
			
				var result = WinApi.WlanSetInterface(
					clientHandle,
					in interfaceId,
					opCode,
					buffer);

				CheckResult(nameof(WinApi.WlanSetInterface), result, false);

				return (WIN32_ERROR)result switch
				{
					WIN32_ERROR.ERROR_SUCCESS => ActionResult.Success,
					WIN32_ERROR.ERROR_INVALID_STATE => ActionResult.NotConnected,
					WIN32_ERROR.ERROR_NOT_FOUND => ActionResult.NotFound,
					WIN32_ERROR.ERROR_NOT_SUPPORTED => ActionResult.NotSupported,
					_ => ActionResult.OtherError
				};
			}
			finally
			{
				WinApi.WlanFreeMemory(pBuffer);
			}
		}
	}
}