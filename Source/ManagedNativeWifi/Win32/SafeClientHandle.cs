using System;
using System.Runtime.InteropServices;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi.Win32;

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
		return (result is ERROR_SUCCESS);
	}
}