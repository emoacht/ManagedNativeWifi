using System;
using System.Text;

// ReSharper disable InconsistentNaming

#pragma warning disable IDE0130
// ReSharper disable once CheckNamespace
namespace Windows.Win32.NetworkManagement.WiFi
#pragma warning restore IDE0130
{
	internal partial struct DOT11_SSID
	{
		public static bool TryCreate(byte[] rawBytes, out DOT11_SSID ssid)
		{
			if (rawBytes is { Length: <= 0 or > 32 })
			{
				ssid = default;
				return false;
			}

			ssid = new DOT11_SSID
			{
				uSSIDLength = (uint)rawBytes.Length,
				ucSSID = rawBytes.AsSpan()
			};

			return true;
		}

		public byte[] ToBytes()
		{
#if NET8_0_OR_GREATER
			return ucSSID.AsReadOnlySpan().ToArray();
#elif NETSTANDARD2_0_OR_GREATER
			return ucSSID.ToArray();
#endif
		}

		public override string ToString()
		{
#if NET8_0_OR_GREATER
			return Encoding.UTF8.GetString(ucSSID.AsReadOnlySpan());
#elif NETSTANDARD2_0_OR_GREATER
			return Encoding.UTF8.GetString(ToBytes());
#endif
		}
	}
}