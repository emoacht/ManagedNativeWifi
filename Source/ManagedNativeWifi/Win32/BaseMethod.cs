using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi.Win32
{
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

			private WLAN_NOTIFICATION_CALLBACK _notificationCallback;

			public WlanNotificationClient() : base()
			{
				RegisterNotification();
			}

			private void RegisterNotification()
			{
				// Storing a delegate in class field is necessary to prevent garbage collector from collecting
				// the delegate before it is called. Otherwise, CallbackOnCollectedDelegate may occur.
				_notificationCallback = new WLAN_NOTIFICATION_CALLBACK((data, context) =>
				{
					var notificationData = Marshal.PtrToStructure<WLAN_NOTIFICATION_DATA>(data);
					if (notificationData.NotificationSource != WLAN_NOTIFICATION_SOURCE_ACM)
						return;

					NotificationReceived?.Invoke(null, notificationData);
				});

				var result = WlanRegisterNotification(
					Handle,
					WLAN_NOTIFICATION_SOURCE_ACM,
					false,
					_notificationCallback,
					IntPtr.Zero,
					IntPtr.Zero,
					0);

				CheckResult(nameof(WlanRegisterNotification), result, true);
			}

			private void UnregisterNotification()
			{
				_notificationCallback = new WLAN_NOTIFICATION_CALLBACK((data, context) => { });

				var result = WlanRegisterNotification(
					Handle,
					WLAN_NOTIFICATION_SOURCE_NONE,
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
					UnregisterNotification();
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

		public static bool Scan(SafeClientHandle clientHandle, Guid interfaceId)
		{
			var result = WlanScan(
				clientHandle,
				interfaceId,
				IntPtr.Zero,
				IntPtr.Zero,
				IntPtr.Zero);

			// ERROR_NDIS_DOT11_POWER_STATE_INVALID will be returned if the interface is turned off.
			return CheckResult(nameof(WlanScan), result, false);
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

				// ERROR_NDIS_DOT11_POWER_STATE_INVALID will be returned if the interface is turned off.
				return CheckResult(nameof(WlanGetAvailableNetworkList), result, false)
					? new WLAN_AVAILABLE_NETWORK_LIST(availableNetworkList).Network
					: new WLAN_AVAILABLE_NETWORK[0];
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

				// ERROR_NDIS_DOT11_POWER_STATE_INVALID will be returned if the interface is turned off.
				return CheckResult(nameof(WlanGetNetworkBssList), result, false)
					? new WLAN_BSS_LIST(wlanBssList).wlanBssEntries
					: new WLAN_BSS_ENTRY[0];
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
				queryData = Marshal.AllocHGlobal(Marshal.SizeOf(ssid));
				Marshal.StructureToPtr(ssid, queryData, false);

				var result = WlanGetNetworkBssList(
					clientHandle,
					interfaceId,
					queryData,
					bssType,
					isSecurityEnabled,
					IntPtr.Zero,
					out wlanBssList);

				// ERROR_NDIS_DOT11_POWER_STATE_INVALID will be returned if the interface is turned off.
				return CheckResult(nameof(WlanGetNetworkBssList), result, false)
					? new WLAN_BSS_LIST(wlanBssList).wlanBssEntries
					: new WLAN_BSS_ENTRY[0];
			}
			finally
			{
				Marshal.FreeHGlobal(queryData);

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

				// ERROR_INVALID_STATE will be returned if the client is not connected to a network.
				return CheckResult(nameof(WlanQueryInterface), result, false)
					? Marshal.PtrToStructure<WLAN_CONNECTION_ATTRIBUTES>(queryData)
					: default(WLAN_CONNECTION_ATTRIBUTES);
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

				return CheckResult(nameof(WlanGetProfileList), result, false)
					? new WLAN_PROFILE_INFO_LIST(profileList).ProfileInfo
					: new WLAN_PROFILE_INFO[0];
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
				out uint grantedAccess);

			profileTypeFlag = flags;

			// ERROR_NOT_FOUND will be returned if the profile is not found.
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

			// ERROR_INVALID_PARAMETER will be returned if the interface is removed.
			// ERROR_ALREADY_EXISTS will be returned if the profile already exists.
			// ERROR_BAD_PROFILE will be returned if the profile xml is not valid.
			// ERROR_NO_MATCH will be returned if the capability specified in the profile is not supported.
			return CheckResult(nameof(WlanSetProfile), result, false, pdwReasonCode);
		}

		public static bool SetProfilePosition(SafeClientHandle clientHandle, Guid interfaceId, string profileName, uint position)
		{
			var result = WlanSetProfilePosition(
				clientHandle,
				interfaceId,
				profileName,
				position,
				IntPtr.Zero);

			// ERROR_INVALID_PARAMETER will be returned if the interface is removed.
			// ERROR_NOT_FOUND will be returned if the position of a profile is invalid.
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

			// ERROR_INVALID_PARAMETER will be returned if the interface is removed.
			// ERROR_NOT_FOUND will be returned if the profile is not found.
			return CheckResult(nameof(WlanRenameProfile), result, false);
		}

		public static bool DeleteProfile(SafeClientHandle clientHandle, Guid interfaceId, string profileName)
		{
			var result = WlanDeleteProfile(
				clientHandle,
				interfaceId,
				profileName,
				IntPtr.Zero);

			// ERROR_INVALID_PARAMETER will be returned if the interface is removed.
			// ERROR_NOT_FOUND will be returned if the profile is not found.
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

			// ERROR_NOT_FOUND will be returned if the interface is removed.
			return CheckResult(nameof(WlanConnect), result, false);
		}

		public static bool Disconnect(SafeClientHandle clientHandle, Guid interfaceId)
		{
			var result = WlanDisconnect(
				clientHandle,
				interfaceId,
				IntPtr.Zero);

			// ERROR_NOT_FOUND will be returned if the interface is removed.
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

				return CheckResult(nameof(WlanGetInterfaceCapability), result, false)
					? Marshal.PtrToStructure<WLAN_INTERFACE_CAPABILITY>(capability)
					: default(WLAN_INTERFACE_CAPABILITY);
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
					: new WLAN_PHY_RADIO_STATE[0];
			}
			finally
			{
				if (queryData != IntPtr.Zero)
					WlanFreeMemory(queryData);
			}
		}

		public static bool SetPhyRadioState(SafeClientHandle clientHandle, Guid interfaceId, WLAN_PHY_RADIO_STATE state)
		{
			var size = Marshal.SizeOf(state);

			IntPtr setData = IntPtr.Zero;
			try
			{
				setData = Marshal.AllocHGlobal(size);
				Marshal.StructureToPtr(state, setData, false);

				var result = WlanSetInterface(
					clientHandle,
					interfaceId,
					WLAN_INTF_OPCODE.wlan_intf_opcode_radio_state,
					(uint)size,
					setData,
					IntPtr.Zero);

				// ERROR_ACCESS_DENIED will be thrown if the caller does not have sufficient permissions.
				// By default, only a user who is logged on as a member of the Administrators group or
				// the Network Configuration Operators group can set the operation mode of the interface.
				// ERROR_GEN_FAILURE will be thrown if the OpCode is not supported by the driver or NIC.
				return CheckResult(nameof(WlanSetInterface), result, false);
			}
			finally
			{
				Marshal.FreeHGlobal(setData);
			}
		}

		public static bool? GetAutoConfig(SafeClientHandle clientHandle, Guid interfaceId)
		{
			var value = GetInterfaceInt(clientHandle, interfaceId, WLAN_INTF_OPCODE.wlan_intf_opcode_autoconf_enabled);

			return value.HasValue
				? (value.Value != 0) // True = other than 0. False = 0.
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
			var setData = IntPtr.Zero;
			try
			{
				setData = Marshal.AllocHGlobal(sizeof(int));
				Marshal.WriteInt32(setData, value);

				var result = WlanSetInterface(
					clientHandle,
					interfaceId,
					wlanIntfOpcode,
					sizeof(int),
					setData,
					IntPtr.Zero);

				return CheckResult(nameof(WlanSetInterface), result, false);
			}
			finally
			{
				Marshal.FreeHGlobal(setData);
			}
		}

		#region Helper

		private static bool CheckResult(string methodName, uint result, bool throwOnFailure, uint reasonCode = 0)
		{
			if (result == ERROR_SUCCESS)
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
				case ERROR_GEN_FAILURE:
					if (!throwOnFailure)
						return false;
					else
						goto default;

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

			var buff = new StringBuilder(512); // This 512 capacity is arbitrary.

			var messageLength = FormatMessage(
			  FORMAT_MESSAGE_FROM_SYSTEM,
			  IntPtr.Zero,
			  errorCode,
			  0x0409, // US (English)
			  buff,
			  buff.Capacity,
			  IntPtr.Zero);

			if (0 < messageLength)
				message.Append($", ErrorMessage: {buff}");

			if (0 < reasonCode)
			{
				message.Append($", ReasonCode: {reasonCode}");

				buff.Clear();

				var result = WlanReasonCodeToString(
					reasonCode,
					buff.Capacity,
					buff,
					IntPtr.Zero);

				if (result == ERROR_SUCCESS)
					message.Append($", ReasonMessage: {buff}");
			}

			return message.ToString();
		}

		#endregion
	}
}
