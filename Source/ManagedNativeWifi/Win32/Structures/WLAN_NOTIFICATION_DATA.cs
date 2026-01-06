using System;
using Windows.Win32.NetworkManagement.WiFi;
// ReSharper disable InconsistentNaming

namespace ManagedNativeWifi.Win32.Structures
{
	internal struct WLAN_NOTIFICATION_DATA
	{
		public WLAN_NOTIFICATION_SOURCES NotificationSource;
		public uint NotificationCode;
		public Guid InterfaceGuid;
		public uint dwDataSize;
		public nint pData;

		public static unsafe WLAN_NOTIFICATION_DATA FromUnmanaged(L2_NOTIFICATION_DATA* data)
		{
			return new WLAN_NOTIFICATION_DATA
			{
				NotificationSource = data->NotificationSource,
				NotificationCode = data->NotificationCode,
				InterfaceGuid = data->InterfaceGuid,
				dwDataSize = data->dwDataSize,
				pData = (nint)data->pData
			};
		}

		public static unsafe explicit operator WLAN_NOTIFICATION_DATA(L2_NOTIFICATION_DATA* data)
			=> FromUnmanaged(data);
	}
}