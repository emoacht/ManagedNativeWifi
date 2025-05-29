using System.Linq;
using System.Text;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi;

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
		this._rawBytes = rawBytes;
		this._rawString = rawString;
	}

	internal NetworkIdentifier(DOT11_SSID ssid) : this(ssid.ToBytes(), ssid.ToString())
	{ }

	internal NetworkIdentifier(DOT11_MAC_ADDRESS bssid) : this(bssid.ToBytes(), bssid.ToString())
	{ }

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
	public NetworkIdentifier(byte[] rawBytes) : this(rawBytes, null)
	{ }

	/// <summary>
	/// Returns the identifier in byte array.
	/// </summary>
	/// <returns>Identifier in byte array</returns>
	public byte[] ToBytes() => _rawBytes?.ToArray();

	/// <summary>
	/// Returns the identifier in string decoded by UTF-8.
	/// </summary>
	/// <returns>Identifier in string decoded by UTF-8</returns>
	public override string ToString() => _rawString;
}