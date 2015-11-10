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
		public class WlanClient : IDisposable
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

				_clientHandle?.Dispose();

				_disposed = true;
			}

			#endregion
		}

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

		public static WLAN_CONNECTION_ATTRIBUTES GetConnectionAttributes(SafeClientHandle clientHandle, Guid interfaceId)
		{
			var queryData = IntPtr.Zero;
			try
			{
				uint dataSize;
				var result = WlanQueryInterface(
					clientHandle,
					interfaceId,
					WLAN_INTF_OPCODE.wlan_intf_opcode_current_connection,
					IntPtr.Zero,
					out dataSize,
					ref queryData,
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

		public static string GetProfile(SafeClientHandle clientHandle, Guid interfaceId, string profileName, out ProfileType profileType)
		{
			var profileXml = IntPtr.Zero;
			try
			{
				uint flags = 0U;
				uint grantedAccess;
				var result = WlanGetProfile(
					clientHandle,
					interfaceId,
					profileName,
					IntPtr.Zero,
					out profileXml,
					ref flags,
					out grantedAccess);

				profileType = Enum.IsDefined(typeof(ProfileType), (int)flags)
					? (ProfileType)(int)flags
					: default(ProfileType);

				// ERROR_NOT_FOUND will be returned if the profile is not found.
				return CheckResult(nameof(WlanGetProfile), result, false)
					? Marshal.PtrToStringUni(profileXml)
					: null; // To be used
			}
			finally
			{
				if (profileXml != IntPtr.Zero)
					WlanFreeMemory(profileXml);
			}
		}

		public static bool SetProfile(SafeClientHandle clientHandle, Guid interfaceId, ProfileType profileType, string profileXml, string profileSecurity, bool overwrite)
		{
			uint pdwReasonCode;
			var result = WlanSetProfile(
				clientHandle,
				interfaceId,
				(uint)profileType,
				profileXml,
				profileSecurity,
				overwrite,
				IntPtr.Zero,
				out pdwReasonCode);

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

		public static void RegisterNotification(SafeClientHandle clientHandle, uint notificationSource, Action<IntPtr, IntPtr> callback)
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

			CheckResult(nameof(WlanRegisterNotification), result, true);
		}

		private static WLAN_NOTIFICATION_CALLBACK _notificationCallback;

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
					if (!throwOnFailure)
						return false;
					else
						goto default;

				case ERROR_ACCESS_DENIED:
					throw new UnauthorizedAccessException(CreateExceptionMessage(methodName, result));

				case ERROR_NOT_ENOUGH_MEMORY:
					throw new OutOfMemoryException(CreateExceptionMessage(methodName, result));

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
	}
}
