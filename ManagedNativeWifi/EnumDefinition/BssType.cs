using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// BSS network type
	/// </summary>
	public enum BssType
	{
		/// <summary>
		/// None (invalid value)
		/// </summary>
		None = 0,

		/// <summary>
		/// Infrastructure BSS network
		/// </summary>
		Infrastructure,

		/// <summary>
		/// Independent BSS (IBSS) network (Ad hoc network)
		/// </summary>
		Independent
	}

	internal static class BssTypeConverter
	{
		public static bool TryConvert(DOT11_BSS_TYPE source, out BssType bssType)
		{
			switch (source)
			{
				case DOT11_BSS_TYPE.dot11_BSS_type_infrastructure:
					bssType = BssType.Infrastructure;
					return true;
				case DOT11_BSS_TYPE.dot11_BSS_type_independent:
					bssType = BssType.Independent;
					return true;
			}
			bssType = default(BssType);
			return false;
		}

		public static bool TryParse(string source, out BssType bssType)
		{
			if (string.Equals("ESS", source, StringComparison.OrdinalIgnoreCase))
			{
				bssType = BssType.Infrastructure;
				return true;
			}
			if (string.Equals("IBSS", source, StringComparison.OrdinalIgnoreCase))
			{
				bssType = BssType.Independent;
				return true;
			}
			bssType = default(BssType);
			return false;
		}

		public static DOT11_BSS_TYPE ConvertBack(BssType source)
		{
			switch (source)
			{
				case BssType.Infrastructure:
					return DOT11_BSS_TYPE.dot11_BSS_type_infrastructure;
				case BssType.Independent:
					return DOT11_BSS_TYPE.dot11_BSS_type_independent;
			}
			throw new ArgumentException(nameof(source));
		}
	}
}