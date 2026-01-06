using System;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace ManagedNativeWifi.Win32
{
	internal static partial class BaseMethod
	{
		public class WlanClient : IDisposable
		{
			public SafeClientHandle Handle { get; }

			public WlanClient()
			{
				unsafe
				{
					var handle = HANDLE.Null;
					var result = WinApi.WlanOpenHandle(
						2, // Client version for Windows Vista and Windows Server 2008
						out _,
						&handle);

					// ERROR_INVALID_PARAMETER: A parameter is incorrect.
					// ERROR_NOT_ENOUGH_MEMORY: Failed to allocate memory to create the client context.
					// ERROR_REMOTE_SESSION_LIMIT_EXCEEDED: Too many handles have been issued by the server.
					CheckResult(nameof(WinApi.WlanOpenHandle), result, true);

					Handle = SafeClientHandle.CreateFromPointer(handle);
				}
			}

			private bool _disposed;

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
					Handle.Dispose();
				}

				_disposed = true;
			}
		}
	}
}