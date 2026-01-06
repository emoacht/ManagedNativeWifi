using ManagedNativeWifi.Win32.Structures;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Windows.Win32;
using Windows.Win32.NetworkManagement.WiFi;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Identifier of wireless LAN
	/// </summary>
	/// <remarks>This class is designed as immutable.</remarks>
	public class NetworkIdentifier
	{
		private readonly byte[] _rawBytes;
		private readonly string _rawString;

		internal NetworkIdentifier(byte[] rawBytes, string rawString)
		{
			_rawBytes = rawBytes;
			_rawString = rawString;
		}

		internal NetworkIdentifier(DOT11_SSID ssid) : this(ssid.ToBytes(), ssid.ToString())
		{ }

		internal NetworkIdentifier(DOT11_MAC_ADDRESS bssid) : this(bssid.ToBytes(), bssid.ToString())
		{ }

		internal NetworkIdentifier(__byte_6 bytes)
		{
			_rawString = "";
			_rawBytes = new byte[6];
			for (var i = 0; i < 6; i++)
			{
				_rawBytes[i] = bytes[i];
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rawString">Raw string</param>
		/// <remarks>Raw byte array will be filled with that encoded by UTF-8.</remarks>
		public NetworkIdentifier(string rawString) : this(Encoding.UTF8.GetBytes(rawString), rawString)
		{ }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rawBytes">Raw byte array</param>
		/// <remarks>Raw string will be left null because byte array cannot always be decoded by UTF-8.</remarks>
		public NetworkIdentifier(byte[] rawBytes) : this(rawBytes, "")
		{ }

	

		/// <summary>
		/// Converts <see cref="System.Net.NetworkInformation.PhysicalAddress"/> to NetworkIdentifier.
		/// </summary>
		public static implicit operator NetworkIdentifier(PhysicalAddress value) => new(value.GetAddressBytes(), "");

		/// <summary>
		/// Converts NetworkIdentifier to <see cref="System.Net.NetworkInformation.PhysicalAddress"/>.
		/// </summary>
		public static implicit operator PhysicalAddress(NetworkIdentifier value) => new(value.ToBytes());

		/// <summary>
		/// Returns the identifier in byte array.
		/// </summary>
		/// <returns>Identifier in byte array</returns>
		public byte[] ToBytes() => _rawBytes.ToArray();

		/// <summary>
		/// Returns the identifier in string decoded by UTF-8.
		/// </summary>
		/// <returns>Identifier in string decoded by UTF-8</returns>
		public override string ToString() => _rawString;
	}
}