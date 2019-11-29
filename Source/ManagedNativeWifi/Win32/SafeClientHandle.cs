using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi.Win32
{
	/// <summary>
	/// Wrapper class only for handle taken by WlanOpenHandle function in Native Wifi API
	/// </summary>
	/// <remarks>
	/// This implementation is based on:
	/// http://referencesource.microsoft.com/#mscorlib/system/runtime/interopservices/safehandle.cs
	/// </remarks>
	internal class SafeClientHandle : SafeHandle
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>This constructor is for P/Invoke.</remarks>
		private SafeClientHandle() : base(IntPtr.Zero, true)
		{ }

		public override bool IsInvalid => (handle == IntPtr.Zero);

		protected override bool ReleaseHandle()
		{
			var result = WlanCloseHandle(handle, IntPtr.Zero);
			return (result == ERROR_SUCCESS);
		}
	}
}