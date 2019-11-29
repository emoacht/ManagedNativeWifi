using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Identifier of wireless LAN
	/// </summary>
	/// <remarks>This class is immutable.</remarks>
	public class NetworkIdentifier
	{
		private readonly byte[] _rawBytes;
		private readonly string _rawString;

		/// <summary>
		/// Constructor
		/// </summary>
		public NetworkIdentifier(byte[] rawBytes, string rawString)
		{
			this._rawBytes = rawBytes;
			this._rawString = rawString;
		}

		internal NetworkIdentifier(DOT11_SSID ssid) : this(ssid.ToBytes(), ssid.ToString())
		{ }

		internal NetworkIdentifier(DOT11_MAC_ADDRESS bssid) : this(bssid.ToBytes(), bssid.ToString())
		{ }

		/// <summary>
		/// Returns the identifier in byte array.
		/// </summary>
		/// <returns>Identifier in byte array</returns>
		public byte[] ToBytes() => _rawBytes?.ToArray();

		/// <summary>
		/// Returns the identifier in UTF-8 string.
		/// </summary>
		/// <returns>Identifier in UTF-8 string</returns>
		public override string ToString() => _rawString;
	}
}