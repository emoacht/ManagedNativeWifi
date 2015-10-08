using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi.Win32
{
	/// <summary>
	/// Wrapper class for handle taken by other functions in Native Wifi API
	/// </summary>
	/// <remarks>
	/// This implementation is based on:
	/// http://referencesource.microsoft.com/#mscorlib/system/runtime/interopservices/safehandle.cs 
	/// </remarks>
	internal class SafeMemoryHandle : SafeHandle
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>This constructor is for P/Invoke.</remarks>
		private SafeMemoryHandle() : base(IntPtr.Zero, true)
		{ }

		public override bool IsInvalid => (handle == IntPtr.Zero);

		protected override bool ReleaseHandle()
		{
			NativeMethod.WlanFreeMemory(handle);
			return true;
		}
	}
}