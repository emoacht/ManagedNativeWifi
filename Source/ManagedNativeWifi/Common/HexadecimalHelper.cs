using System;

namespace ManagedNativeWifi.Common;

internal static class HexadecimalHelper
{
	/// <summary>
	/// Converts a string which represents a byte array in hexadecimal format to the byte array.
	/// </summary>
	/// <param name="source">Hexadecimal string</param>
	/// <returns>Original byte array</returns>
	/// <exception cref="System.FormatException">
	/// Thrown when the string is not in hexadecimal format.
	/// </exception>
	public static byte[] ToBytes(string source)
	{
		if (string.IsNullOrWhiteSpace(source))
			return Array.Empty<byte>();

		var buffer = new byte[source.Length / 2];

		for (int i = 0; i < buffer.Length; i++)
		{
			try
			{
				buffer[i] = Convert.ToByte(source.Substring(i * 2, 2), 16);
			}
			catch (FormatException)
			{
				break;
			}
		}
		return buffer;
	}

	/// <summary>
	/// Converts a byte array to a string which represents the byte array in hexadecimal format.
	/// </summary>
	/// <param name="source">Original byte array</param>
	/// <returns>Hexadecimal string</returns>
	public static string ToString(byte[] source) =>
		BitConverter.ToString(source).Replace("-", "");
}