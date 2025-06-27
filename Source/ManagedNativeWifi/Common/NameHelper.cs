using System.Text;

namespace ManagedNativeWifi.Common;

internal static class NameHelper
{
	/// <summary>
	/// Converts a pascal camel case string to an upper hyphenated string.
	/// </summary>
	/// <param name="pascalCamelCaseString">Pascal camel case string</param>
	/// <returns>Upper hyphenated string</returns>
	/// <remarks>
	/// For example, CamelCaseString is coverted to CAMEL-CASE-STRING.
	/// </remarks>
	public static string ToUpperHyphenated(string pascalCamelCaseString)
	{
		const char separator = '-';

		if (string.IsNullOrWhiteSpace(pascalCamelCaseString))
			return string.Empty;

		var array = pascalCamelCaseString.ToCharArray();
		var buffer = new StringBuilder();

		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0)
			{
				if (char.IsUpper(array[i]) ||
					(char.IsDigit(array[i]) && !char.IsDigit(array[i - 1])))
				{
					buffer.Append(separator);
				}
			}
			buffer.Append(char.ToUpper(array[i]));
		}
		return buffer.ToString();
	}
}