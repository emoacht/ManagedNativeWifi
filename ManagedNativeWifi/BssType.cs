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
		/// None
		/// </summary>
		None = 0,

		/// <summary>
		/// Infrastructure BSS network
		/// </summary>
		Infrastructure,

		/// <summary>
		/// Independent BSS (IBSS) network (Ad hoc network)
		/// </summary>
		Independent,

		/// <summary>
		/// Any BSS network
		/// </summary>
		Any
	}

	internal static class BssTypeConverter
	{
		public static BssType ToBssType(DOT11_BSS_TYPE source)
		{
			switch (source)
			{
				case DOT11_BSS_TYPE.dot11_BSS_type_infrastructure:
					return BssType.Infrastructure;
				case DOT11_BSS_TYPE.dot11_BSS_type_independent:
					return BssType.Independent;
				default:
					return BssType.Any;
			}
		}

		public static DOT11_BSS_TYPE FromBssType(BssType source)
		{
			switch (source)
			{
				case BssType.Infrastructure:
					return DOT11_BSS_TYPE.dot11_BSS_type_infrastructure;
				case BssType.Independent:
					return DOT11_BSS_TYPE.dot11_BSS_type_independent;
				default:
					return DOT11_BSS_TYPE.dot11_BSS_type_any;
			}
		}

		public static BssType Parse(string source)
		{
			if (string.IsNullOrWhiteSpace(source))
			{
				return default(BssType);
			}
			if (string.Equals("ESS", source, StringComparison.OrdinalIgnoreCase))
			{
				return BssType.Infrastructure;
			}
			if (string.Equals("IBSS", source, StringComparison.OrdinalIgnoreCase))
			{
				return BssType.Independent;
			}
			return BssType.Any;
		}
	}
}