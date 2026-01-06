using ManagedNativeWifi.Win32.Structures;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Win32;
using Windows.Win32.NetworkManagement.WiFi;
// ReSharper disable InconsistentNaming

#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace ManagedNativeWifi.Win32
{
	internal static partial class BaseMethod
	{
		public unsafe class WlanNotificationClient : WlanClient
		{
			private static readonly ConcurrentDictionary<int, Action<nint>> HandlersMap = new();
			private static int _nextId; //todo: overflow?

			// For .NET Standard we need to keep a reference to the delegate so GC won't collect it
			#if NETSTANDARD2_0_OR_GREATER
			[UnmanagedFunctionPointer(CallingConvention.StdCall)]
			private delegate void WlanNotificationCallbackDelegate(L2_NOTIFICATION_DATA* data, void* context);

			private static readonly WlanNotificationCallbackDelegate _wlanNotificationCallbackDelegate = WlanNotificationCallback;
			#endif

			private int _currentDelegateId;
			private WLAN_NOTIFICATION_SOURCES _notificationSource;
	
			public event EventHandler<WLAN_NOTIFICATION_DATA> NotificationReceived;

			/// <summary>
			/// Registers or appends notification source.
			/// </summary>
			/// <param name="notificationSources">Notification sources</param>
			public void Register(WLAN_NOTIFICATION_SOURCES notificationSources)
			{
				var existingSource = _notificationSource;
				_notificationSource |= notificationSources;
				if (existingSource == _notificationSource)
					return;

				_currentDelegateId = Interlocked.Increment(ref _nextId);
				if (!HandlersMap.TryAdd(_currentDelegateId, NotificationHandler))
					throw new OverflowException("Failed to add notification handler.");


				var result = WinApi.WlanRegisterNotification(
					Handle,
					_notificationSource,
					false,
			#if NET8_0_OR_GREATER
					&WlanNotificationCallback,
			#elif NETSTANDARD2_0_OR_GREATER
					(delegate *unmanaged[Stdcall]<L2_NOTIFICATION_DATA*,void*,void>)Marshal.GetFunctionPointerForDelegate(_wlanNotificationCallbackDelegate),
			#endif
					(void*)_currentDelegateId
				);

				// ERROR_INVALID_HANDLE: The client handle was not found in the handle table.
				// ERROR_INVALID_PARAMETER: A parameter is incorrect.
				// ERROR_NOT_ENOUGH_MEMORY: Failed to allocate memory for the query results.
				// ERROR_ACCESS_DENIED: When WLAN_NOTIFICATION_SOURCE_MSM flag is set in dwNotifSource,
				// the wiFiControl device capability is required but that capability is not granted.
				// Requesting the wiFiControl device capability will require consent from the user
				// regarding location access.
				CheckResult(nameof(WinApi.WlanRegisterNotification), result, true);
			}
			
			private void NotificationHandler(IntPtr data)
			{
				if (data == IntPtr.Zero) return;

				var notificationData = (L2_NOTIFICATION_DATA*)data;
				if (_notificationSource.HasFlag(notificationData->NotificationSource))
				{
					NotificationReceived?.Invoke(this, (WLAN_NOTIFICATION_DATA)notificationData);
				}
			}

			/// <summary>
			/// Unregister notification source.
			/// </summary>
			private void Unregister()
			{
				var result = WinApi.WlanRegisterNotification(
					Handle,
					WLAN_NOTIFICATION_SOURCES.WLAN_NOTIFICATION_SOURCE_NONE,
					false,
					null,
					null);

				CheckResult(nameof(WinApi.WlanRegisterNotification), result, true);

				HandlersMap.TryRemove(_currentDelegateId, out _);
			}
	#if NET8_0_OR_GREATER
			[UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
	#endif
			private static void WlanNotificationCallback(L2_NOTIFICATION_DATA* data, void* context)
			{
				var delegateId = (int)context;

				if (HandlersMap.TryGetValue(delegateId, out var notificationHandler))
				{
					notificationHandler.Invoke((nint)data);
				}
			}

			#region Dispose

			private bool _disposed;

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				if (disposing)
				{
					NotificationReceived = null;

					// Since closing the handle used for a registration to receive notification will
					// automatically remove the registration, an unregistration is not actually required.
					Unregister();
				}

				_disposed = true;

				base.Dispose(disposing);
			}

			#endregion
		}
	}
}