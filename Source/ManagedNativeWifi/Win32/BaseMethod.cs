using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi.Win32;

internal static class BaseMethod
{
	#region Client

	public class WlanClient : IDisposable
	{
		private readonly SafeClientHandle _clientHandle = null;

		public SafeClientHandle Handle => _clientHandle;

		public WlanClient()
		{
			var result = WlanOpenHandle(
				2, // Client version for Windows Vista and Windows Server 2008
				IntPtr.Zero,
				out _,
				out _clientHandle);

			// ERROR_INVALID_PARAMETER: A parameter is incorrect.
			// ERROR_NOT_ENOUGH_MEMORY: Failed to allocate memory to create the client context.
			// ERROR_REMOTE_SESSION_LIMIT_EXCEEDED: Too many handles have been issued by the server.
			CheckResult(nameof(WlanOpenHandle), result, true);
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

			if (disposing)
			{
				_clientHandle?.Dispose();
			}

			_disposed = true;
		}

		#endregion
	}

	public class WlanNotificationClient : WlanClient
	{
		public event EventHandler<WLAN_NOTIFICATION_DATA> NotificationReceived;

		private WLAN_NOTIFICATION_SOURCE _notificationSource;
		private WLAN_NOTIFICATION_CALLBACK _notificationCallback;

		public WlanNotificationClient() : base()
		{ }

		public void Register(WLAN_NOTIFICATION_SOURCE notificationSource)
		{
			_notificationSource = notificationSource;

			// Storing a delegate in class field is necessary to prevent garbage collector from collecting
			// the delegate before it is called. Otherwise, CallbackOnCollectedDelegate may occur.
			_notificationCallback = new WLAN_NOTIFICATION_CALLBACK((data, _) =>
			{
				var notificationData = Marshal.PtrToStructure<WLAN_NOTIFICATION_DATA>(data);
				if (_notificationSource.HasFlag(notificationData.NotificationSource))
				{
					NotificationReceived?.Invoke(null, notificationData);
				}
			});

			var result = WlanRegisterNotification(
				Handle,
				_notificationSource,
				false,
				_notificationCallback,
				IntPtr.Zero,
				IntPtr.Zero,
				0);

			// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect.
			// ERROR_NOT_ENOUGH_MEMORY: Failed to allocate memory for the query results.
			CheckResult(nameof(WlanRegisterNotification), result, true);
		}

		private void Unregister()
		{
			_notificationCallback = new WLAN_NOTIFICATION_CALLBACK((data, _) => { });

			var result = WlanRegisterNotification(
				Handle,
				WLAN_NOTIFICATION_SOURCE.WLAN_NOTIFICATION_SOURCE_NONE,
				false,
				_notificationCallback,
				IntPtr.Zero,
				IntPtr.Zero,
				0);

			CheckResult(nameof(WlanRegisterNotification), result, true);
		}

		#region Dispose

		private bool _disposed = false;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				NotificationReceived = null;

				// Since closing the handle used for a registration to receive notification will
				// automatically undone the registration, an unregistration is not actually required.
				Unregister();
			}

			_disposed = true;

			base.Dispose(disposing);
		}

		#endregion
	}

	#endregion

	public static IEnumerable<WLAN_INTERFACE_INFO> GetInterfaceInfoList(SafeClientHandle clientHandle)
	{
		var interfaceList = IntPtr.Zero;
		try
		{
			var result = WlanEnumInterfaces(
				clientHandle,
				IntPtr.Zero,
				out interfaceList);

			// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect.
			// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request.
			return CheckResult(nameof(WlanEnumInterfaces), result, true)
				? new WLAN_INTERFACE_INFO_LIST(interfaceList).InterfaceInfo
				: null; // Not to be used
		}
		finally
		{
			if (interfaceList != IntPtr.Zero)
				WlanFreeMemory(interfaceList);
		}
	}

	public static bool Scan(SafeClientHandle clientHandle, Guid interfaceId, DOT11_SSID ssid)
	{
		var queryData = IntPtr.Zero;
		try
		{
			if (!ssid.Equals(default(DOT11_SSID)))
			{
				queryData = WlanAllocateMemory((uint)Marshal.SizeOf(ssid));
				Marshal.StructureToPtr(ssid, queryData, false);
			}

			var result = WlanScan(
				clientHandle,
				interfaceId,
				queryData,
				IntPtr.Zero,
				IntPtr.Zero);

			// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_NOT_ENOUGH_MEMORY: Failed to allocate memory for the query results.
			// ERROR_NDIS_DOT11_POWER_STATE_INVALID: The interface is turned off.
			// ERROR_BUSY: The requested resource is in use.
			return CheckResult(nameof(WlanScan), result, false);
		}
		finally
		{
			if (queryData != IntPtr.Zero)
				WlanFreeMemory(queryData);
		}
	}

	public static IEnumerable<WLAN_AVAILABLE_NETWORK> GetAvailableNetworkList(SafeClientHandle clientHandle, Guid interfaceId)
	{
		var availableNetworkList = IntPtr.Zero;
		try
		{
			var result = WlanGetAvailableNetworkList(
				clientHandle,
				interfaceId,
				WLAN_AVAILABLE_NETWORK_INCLUDE_ALL_MANUAL_HIDDEN_PROFILES,
				IntPtr.Zero,
				out availableNetworkList);

			// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request.
			// ERROR_NDIS_DOT11_POWER_STATE_INVALID: The interface is turned off.
			return CheckResult(nameof(WlanGetAvailableNetworkList), result, false)
				? new WLAN_AVAILABLE_NETWORK_LIST(availableNetworkList).Network
				: Enumerable.Empty<WLAN_AVAILABLE_NETWORK>();
		}
		finally
		{
			if (availableNetworkList != IntPtr.Zero)
				WlanFreeMemory(availableNetworkList);
		}
	}

	public static IEnumerable<WLAN_BSS_ENTRY> GetNetworkBssEntryList(SafeClientHandle clientHandle, Guid interfaceId)
	{
		var wlanBssList = IntPtr.Zero;
		try
		{
			var result = WlanGetNetworkBssList(
				clientHandle,
				interfaceId,
				IntPtr.Zero,
				DOT11_BSS_TYPE.dot11_BSS_type_any,
				false,
				IntPtr.Zero,
				out wlanBssList);

			// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request.
			// ERROR_NDIS_DOT11_POWER_STATE_INVALID: The interface is turned off.
			// ERROR_NOT_FOUND: The inteface GUID could not be found.
			// ERROR_NOT_SUPPORTED: The WLAN AutoConfig service is disabled.
			// ERROR_SERVICE_NOT_ACTIVE: The WLAN AutoConfig service has not been started.
			return CheckResult(nameof(WlanGetNetworkBssList), result, false)
				? new WLAN_BSS_LIST(wlanBssList).wlanBssEntries
				: Enumerable.Empty<WLAN_BSS_ENTRY>();
		}
		finally
		{
			if (wlanBssList != IntPtr.Zero)
				WlanFreeMemory(wlanBssList);
		}
	}

	public static WLAN_BSS_ENTRY[] GetNetworkBssEntryList(SafeClientHandle clientHandle, Guid interfaceId, DOT11_SSID ssid, DOT11_BSS_TYPE bssType, bool isSecurityEnabled)
	{
		var queryData = IntPtr.Zero;
		var wlanBssList = IntPtr.Zero;
		try
		{
			queryData = WlanAllocateMemory((uint)Marshal.SizeOf(ssid));
			Marshal.StructureToPtr(ssid, queryData, false);

			var result = WlanGetNetworkBssList(
				clientHandle,
				interfaceId,
				queryData,
				bssType,
				isSecurityEnabled,
				IntPtr.Zero,
				out wlanBssList);

			return CheckResult(nameof(WlanGetNetworkBssList), result, false)
				? new WLAN_BSS_LIST(wlanBssList).wlanBssEntries
				: Array.Empty<WLAN_BSS_ENTRY>();
		}
		finally
		{
			WlanFreeMemory(queryData);

			if (wlanBssList != IntPtr.Zero)
				WlanFreeMemory(wlanBssList);
		}
	}

	public static WLAN_CONNECTION_ATTRIBUTES GetConnectionAttributes(SafeClientHandle clientHandle, Guid interfaceId)
	{
		var queryData = IntPtr.Zero;
		try
		{
			var result = WlanQueryInterface(
				clientHandle,
				interfaceId,
				WLAN_INTF_OPCODE.wlan_intf_opcode_current_connection,
				IntPtr.Zero,
				out _,
				out queryData,
				IntPtr.Zero);

			// ERROR_INVALID_STATE: The client is not connected to a network.
			return CheckResult(nameof(WlanQueryInterface), result, false)
				? Marshal.PtrToStructure<WLAN_CONNECTION_ATTRIBUTES>(queryData)
				: default;
		}
		finally
		{
			if (queryData != IntPtr.Zero)
				WlanFreeMemory(queryData);
		}
	}

	public static IEnumerable<WLAN_PROFILE_INFO> GetProfileInfoList(SafeClientHandle clientHandle, Guid interfaceId)
	{
		var profileList = IntPtr.Zero;
		try
		{
			var result = WlanGetProfileList(
				clientHandle,
				interfaceId,
				IntPtr.Zero,
				out profileList);

			// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			// ERROR_NOT_ENOUGH_MEMORY: Not enough memory is available to process this request.
			return CheckResult(nameof(WlanGetProfileList), result, false)
				? new WLAN_PROFILE_INFO_LIST(profileList).ProfileInfo
				: Enumerable.Empty<WLAN_PROFILE_INFO>();
		}
		finally
		{
			if (profileList != IntPtr.Zero)
				WlanFreeMemory(profileList);
		}
	}

	public static string GetProfile(SafeClientHandle clientHandle, Guid interfaceId, string profileName, out uint profileTypeFlag)
	{
		uint flags = 0U;
		var result = WlanGetProfile(
			clientHandle,
			interfaceId,
			profileName,
			IntPtr.Zero,
			out string profileXml,
			ref flags,
			out _);

		profileTypeFlag = flags;

		// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
		// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
		// ERROR_NOT_ENOUGH_MEMORY: Not enough storage is available to process this command.
		// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
		// ERROR_NOT_FOUND: The profile is not found.
		return CheckResult(nameof(WlanGetProfile), result, false)
			? profileXml
			: null; // To be used
	}

	public static bool SetProfile(SafeClientHandle clientHandle, Guid interfaceId, uint profileTypeFlag, string profileXml, string profileSecurity, bool overwrite)
	{
		var result = WlanSetProfile(
			clientHandle,
			interfaceId,
			profileTypeFlag,
			profileXml,
			profileSecurity,
			overwrite,
			IntPtr.Zero,
			out uint pdwReasonCode);

		// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
		// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
		// ERROR_ALREADY_EXISTS: The profile already exists.
		// ERROR_BAD_PROFILE: The profile XML is not valid.
		// ERROR_NO_MATCH: The capabilities specified in the profile is not supported by the interface.
		return CheckResult(nameof(WlanSetProfile), result, false, pdwReasonCode);
	}

	public static bool SetProfileEapXmlUserData(SafeClientHandle clientHandle, Guid interfaceId, string profileName, uint eapXmlFlag, string userDataXml)
	{
		var result = WlanSetProfileEapXmlUserData(
			clientHandle,
			interfaceId,
			profileName,
			eapXmlFlag,
			userDataXml,
			IntPtr.Zero);

		// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
		// ERROR_INVALID_PARAMETER: A parameter is incorrect.
		// ERROR_NOT_ENOUGH_MEMORY: Not enough storage is available to process this command.
		// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
		// ERROR_BAD_PROFILE: The EAP XML is not valid.
		// ERROR_NOT_SUPPORTED: The profile does not allow storage of user data.
		// ERROR_SERVICE_NOT_ACTIVE: The WLAN service has not been started.
		return CheckResult(nameof(WlanSetProfileEapXmlUserData), result, false);
	}

	public static bool SetProfilePosition(SafeClientHandle clientHandle, Guid interfaceId, string profileName, uint position)
	{
		var result = WlanSetProfilePosition(
			clientHandle,
			interfaceId,
			profileName,
			position,
			IntPtr.Zero);

		// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
		// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
		// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
		// ERROR_NOT_FOUND: The position of a profile is invalid.
		return CheckResult(nameof(WlanSetProfilePosition), result, false);
	}

	public static bool RenameProfile(SafeClientHandle clientHandle, Guid interfaceId, string oldProfileName, string newProfileName)
	{
		var result = WlanRenameProfile(
			clientHandle,
			interfaceId,
			oldProfileName,
			newProfileName,
			IntPtr.Zero);

		// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
		// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
		// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
		// ERROR_NOT_FOUND: The profile is not found in the profile store.
		return CheckResult(nameof(WlanRenameProfile), result, false);
	}

	public static bool DeleteProfile(SafeClientHandle clientHandle, Guid interfaceId, string profileName)
	{
		var result = WlanDeleteProfile(
			clientHandle,
			interfaceId,
			profileName,
			IntPtr.Zero);

		// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
		// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
		// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
		// ERROR_NOT_FOUND: The profile is not found in the profile store.
		return CheckResult(nameof(WlanDeleteProfile), result, false);
	}

	public static bool Connect(SafeClientHandle clientHandle, Guid interfaceId, string profileName, DOT11_BSS_TYPE bssType)
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
			interfaceId,
			ref connectionParameters,
			IntPtr.Zero);

		// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
		// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
		// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
		// ERROR_NOT_FOUND: The interface is removed.
		return CheckResult(nameof(WlanConnect), result, false);
	}

	public static bool Connect(SafeClientHandle clientHandle, Guid interfaceId, string profileName, DOT11_BSS_TYPE bssType, DOT11_MAC_ADDRESS bssid)
	{
		var bssidList = new DOT11_BSSID_LIST();
		bssidList.Header = new NDIS_OBJECT_HEADER()
		{
			Type = 0x80, //NDIS_OBJECT_TYPE_DEFAULT
			Revision = 1, //DOT11_BSSID_LIST_REVISION_1
			Size = Convert.ToUInt16(Marshal.SizeOf(bssidList))
		};
		bssidList.uNumOfEntries = 1;
		bssidList.uTotalNumOfEntries = 1;
		bssidList.BSSIDs = bssid;

		var connectionParameters = new WLAN_CONNECTION_PARAMETERS
		{
			wlanConnectionMode = WLAN_CONNECTION_MODE.wlan_connection_mode_profile,
			strProfile = profileName,
			dot11BssType = bssType,
			dwFlags = 0U
		};

		try
		{
			connectionParameters.pDesiredBssidList = WlanAllocateMemory((uint)Marshal.SizeOf(bssidList));
			Marshal.StructureToPtr(bssidList, connectionParameters.pDesiredBssidList, false);

			var result = WlanConnect(
				clientHandle,
				interfaceId,
				ref connectionParameters,
				IntPtr.Zero);

			return CheckResult(nameof(WlanConnect), result, false);
		}
		finally
		{
			WlanFreeMemory(connectionParameters.pDesiredBssidList);
		}
	}

	public static bool Disconnect(SafeClientHandle clientHandle, Guid interfaceId)
	{
		var result = WlanDisconnect(
			clientHandle,
			interfaceId,
			IntPtr.Zero);

		// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
		// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
		// ERROR_NOT_ENOUGH_MEMORY: Failed to allocate memory for the query results.
		// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
		// ERROR_NOT_FOUND: The interface is removed.
		return CheckResult(nameof(WlanDisconnect), result, false);
	}

	public static WLAN_INTERFACE_CAPABILITY GetInterfaceCapability(SafeClientHandle clientHandle, Guid interfaceId)
	{
		var capability = IntPtr.Zero;
		try
		{
			var result = WlanGetInterfaceCapability(
				clientHandle,
				interfaceId,
				IntPtr.Zero,
				out capability);

			// ERROR_INVALID_HANDLE: The client handle is not found in the handle table.
			// ERROR_INVALID_PARAMETER: A parameter is incorrect. The interface is removed.
			return CheckResult(nameof(WlanGetInterfaceCapability), result, false)
				? Marshal.PtrToStructure<WLAN_INTERFACE_CAPABILITY>(capability)
				: default;
		}
		finally
		{
			if (capability != IntPtr.Zero)
				WlanFreeMemory(capability);
		}
	}

	public static IEnumerable<WLAN_PHY_RADIO_STATE> GetPhyRadioStates(SafeClientHandle clientHandle, Guid interfaceId)
	{
		var queryData = IntPtr.Zero;
		try
		{
			var result = WlanQueryInterface(
				clientHandle,
				interfaceId,
				WLAN_INTF_OPCODE.wlan_intf_opcode_radio_state,
				IntPtr.Zero,
				out _,
				out queryData,
				IntPtr.Zero);

			return CheckResult(nameof(WlanQueryInterface), result, false)
				? new WLAN_RADIO_STATE(queryData).PhyRadioState
				: Enumerable.Empty<WLAN_PHY_RADIO_STATE>();
		}
		finally
		{
			if (queryData != IntPtr.Zero)
				WlanFreeMemory(queryData);
		}
	}

	public static bool SetPhyRadioState(SafeClientHandle clientHandle, Guid interfaceId, WLAN_PHY_RADIO_STATE state)
	{
		var size = (uint)Marshal.SizeOf(state);
		var setData = IntPtr.Zero;
		try
		{
			setData = WlanAllocateMemory(size);
			Marshal.StructureToPtr(state, setData, false);

			var result = WlanSetInterface(
				clientHandle,
				interfaceId,
				WLAN_INTF_OPCODE.wlan_intf_opcode_radio_state,
				size,
				setData,
				IntPtr.Zero);

			// ERROR_ACCESS_DENIED: The caller does not have sufficient permissions.
			// By default, only a user who is logged on as a member of the Administrators group or
			// the Network Configuration Operators group can set the operation mode of the interface.
			// ERROR_GEN_FAILURE: The OpCode is not supported by the driver or NIC.
			return CheckResult(nameof(WlanSetInterface), result, false);
		}
		finally
		{
			WlanFreeMemory(setData);
		}
	}

	public static bool? IsAutoConfig(SafeClientHandle clientHandle, Guid interfaceId)
	{
		var value = GetInterfaceInt(clientHandle, interfaceId, WLAN_INTF_OPCODE.wlan_intf_opcode_autoconf_enabled);

		return value.HasValue
			? (value.Value is not 0) // True = other than 0. False = 0.
			: (bool?)null;
	}

	private static int? GetInterfaceInt(SafeClientHandle clientHandle, Guid interfaceId, WLAN_INTF_OPCODE wlanIntfOpcode)
	{
		var queryData = IntPtr.Zero;
		try
		{
			var result = WlanQueryInterface(
				clientHandle,
				interfaceId,
				wlanIntfOpcode,
				IntPtr.Zero,
				out _,
				out queryData,
				IntPtr.Zero);

			return CheckResult(nameof(WlanQueryInterface), result, false)
				? Marshal.ReadInt32(queryData)
				: (int?)null;
		}
		finally
		{
			if (queryData != IntPtr.Zero)
				WlanFreeMemory(queryData);
		}
	}

	private static bool SetInterfaceInt(SafeClientHandle clientHandle, Guid interfaceId, WLAN_INTF_OPCODE wlanIntfOpcode, int value)
	{
		var size = (uint)Marshal.SizeOf(value);
		var setData = IntPtr.Zero;
		try
		{
			setData = WlanAllocateMemory(size);
			Marshal.WriteInt32(setData, value);

			var result = WlanSetInterface(
				clientHandle,
				interfaceId,
				wlanIntfOpcode,
				size,
				setData,
				IntPtr.Zero);

			return CheckResult(nameof(WlanSetInterface), result, false);
		}
		finally
		{
			WlanFreeMemory(setData);
		}
	}

	#region Helper

	public static bool ThrowsOnAnyFailure { get; set; }

	private static bool CheckResult(string methodName, uint result, bool throwOnFailure, uint reasonCode = 0)
	{
		if (result is ERROR_SUCCESS)
			return true;

		switch (result)
		{
			case ERROR_INVALID_PARAMETER:
			case ERROR_INVALID_STATE:
			case ERROR_NOT_FOUND:
			case ERROR_NOT_SUPPORTED:
			case ERROR_SERVICE_NOT_ACTIVE:
			case ERROR_NDIS_DOT11_AUTO_CONFIG_ENABLED:
			case ERROR_NDIS_DOT11_MEDIA_IN_USE:
			case ERROR_NDIS_DOT11_POWER_STATE_INVALID:
			case ERROR_BUSY:
			case ERROR_GEN_FAILURE:
				if (throwOnFailure || ThrowsOnAnyFailure)
					goto default;
				else
					return false;

			case ERROR_ACCESS_DENIED:
				throw new UnauthorizedAccessException(CreateExceptionMessage(methodName, result));

			case ERROR_NOT_ENOUGH_MEMORY:
				throw new OutOfMemoryException(CreateExceptionMessage(methodName, result)); // To be considered

			case ERROR_INVALID_HANDLE:
			case ERROR_ALREADY_EXISTS:
			case ERROR_BAD_PROFILE:
			case ERROR_NO_MATCH:
			default:
				throw new Win32Exception((int)result, CreateExceptionMessage(methodName, result, reasonCode));
		}
	}

	private static string CreateExceptionMessage(string methodName, uint errorCode, uint reasonCode = 0)
	{
		var message = new StringBuilder($"MethodName: {methodName}, ErrorCode: {errorCode}");

		var buffer = new StringBuilder(512); // This 512 capacity is arbitrary.

		var messageLength = FormatMessage(
			FORMAT_MESSAGE_FROM_SYSTEM,
			IntPtr.Zero,
			errorCode,
			0x0409, // US (English)
			buffer,
			buffer.Capacity,
			IntPtr.Zero);

		if (0 < messageLength)
			message.Append($", ErrorMessage: {buffer}");

		if (0 < reasonCode)
		{
			message.Append($", ReasonCode: {reasonCode}");

			buffer.Clear();

			var result = WlanReasonCodeToString(
				reasonCode,
				buffer.Capacity,
				buffer,
				IntPtr.Zero);

			if (result is ERROR_SUCCESS)
				message.Append($", ReasonMessage: {buffer}");
		}

		return message.ToString();
	}

	#endregion
}