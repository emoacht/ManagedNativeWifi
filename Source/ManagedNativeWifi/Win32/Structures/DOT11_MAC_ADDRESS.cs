using System;
using System.Linq;
using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace ManagedNativeWifi.Win32.Structures
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct DOT11_MAC_ADDRESS
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public byte[] ucDot11MacAddress;

		/// <summary>
		/// Returns the byte array of MAC address
		/// </summary>
		/// <returns></returns>
		public byte[] ToBytes() => ucDot11MacAddress?.ToArray() ?? [];

		/// <summary>
		/// Returns the hexadecimal string representation of MAC address delimited by colon.
		/// </summary>
		/// <returns>Hexadecimal string</returns>
		public override string ToString()
		{
			return (ucDot11MacAddress is not null)
				? BitConverter.ToString(ucDot11MacAddress).Replace('-', ':')
				: string.Empty;
		}

		internal static bool TryCreate(byte[] rawBytes, out DOT11_MAC_ADDRESS bssid)
		{
			if (rawBytes is not { Length: 6 })
			{
				bssid = default;
				return false;
			}

			bssid = new DOT11_MAC_ADDRESS { ucDot11MacAddress = rawBytes };
			return true;
		}
	}
}
