using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// PHY and media type
	/// </summary>
	/// <remarks>
	/// Equivalent to DOT11_PHY_TYPE:
	/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/dot11-phy-type
	/// https://docs.microsoft.com/en-us/windows-hardware/drivers/ddi/windot11/ne-windot11-_dot11_phy_type
	/// </remarks>
	public enum PhyType
	{
		/// <summary>
		/// Unknown or uninitialized PHY
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Unknown or uninitialized PHY
		/// </summary>
		Any,

		/// <summary>
		/// Frequency-hopping spread spectrum (FHSS) PHY
		/// </summary>
		Fhss,

		/// <summary>
		/// Direct-sequence spread spectrum (DSSS) PHY
		/// </summary>
		Dsss,

		/// <summary>
		/// Infrared (IR) baseband PHY
		/// </summary>
		IrBaseband,

		/// <summary>
		/// Orthogonal frequency division multiplexing (OFDM) 802.11a PHY
		/// </summary>
		Ofdm,

		/// <summary>
		/// High-rate DSSS (HRDSSS) 802.11b PHY
		/// </summary>
		HrDsss,

		/// <summary>
		/// Extended-rate (ERP) 802.11g PHY
		/// </summary>
		Erp,

		/// <summary>
		/// High-throughput (HT) 802.11n PHY
		/// Each 802.11n PHY, whether dual-band or not, is specified as this PHY type.
		/// </summary>
		Ht,

		/// <summary>
		/// Very high-throughput (VHT) 802.11ac PHY
		/// </summary>
		Vht,

		/// <summary>
		/// Directional Multi-Gigabit (DMG) 802.11ad PHY
		/// </summary>
		Dmg,

		/// <summary>
		/// High Efficiency (HE) 802.11ax PHY
		/// </summary>
		He,

		/// <summary>
		/// Extremely high-throughput (EHT) 802.11be PHY
		/// </summary>
		Eht,

		/// <summary>
		/// The start of the range that is used to define proprietary PHY types that are developed by
		/// an independent hardware vendor (IHV)
		/// </summary>
		IhvStart,

		/// <summary>
		/// The end of the range that is used to define proprietary PHY types that are developed by
		/// an IHV
		/// </summary>
		IhvEnd
	}

	internal static class PhyTypeConverter
	{
		public static PhyType Convert(DOT11_PHY_TYPE source)
		{
			return source switch
			{
				DOT11_PHY_TYPE.dot11_phy_type_any => PhyType.Any,
				DOT11_PHY_TYPE.dot11_phy_type_fhss => PhyType.Fhss,
				DOT11_PHY_TYPE.dot11_phy_type_dsss => PhyType.Dsss,
				DOT11_PHY_TYPE.dot11_phy_type_irbaseband => PhyType.IrBaseband,
				DOT11_PHY_TYPE.dot11_phy_type_ofdm => PhyType.Ofdm,
				DOT11_PHY_TYPE.dot11_phy_type_hrdsss => PhyType.HrDsss,
				DOT11_PHY_TYPE.dot11_phy_type_erp => PhyType.Erp,
				DOT11_PHY_TYPE.dot11_phy_type_ht => PhyType.Ht,
				DOT11_PHY_TYPE.dot11_phy_type_vht => PhyType.Vht,
				DOT11_PHY_TYPE.dot11_phy_type_dmg => PhyType.Dmg,
				DOT11_PHY_TYPE.dot11_phy_type_he => PhyType.He,
				DOT11_PHY_TYPE.dot11_phy_type_eht => PhyType.Eht,
				DOT11_PHY_TYPE.dot11_phy_type_IHV_start => PhyType.IhvStart,
				DOT11_PHY_TYPE.dot11_phy_type_IHV_end => PhyType.IhvEnd,
				_ => PhyType.Unknown,
			};
		}

		public static DOT11_PHY_TYPE ConvertBack(PhyType source)
		{
			return source switch
			{
				PhyType.Any => DOT11_PHY_TYPE.dot11_phy_type_any,
				PhyType.Fhss => DOT11_PHY_TYPE.dot11_phy_type_fhss,
				PhyType.Dsss => DOT11_PHY_TYPE.dot11_phy_type_dsss,
				PhyType.IrBaseband => DOT11_PHY_TYPE.dot11_phy_type_irbaseband,
				PhyType.Ofdm => DOT11_PHY_TYPE.dot11_phy_type_ofdm,
				PhyType.HrDsss => DOT11_PHY_TYPE.dot11_phy_type_hrdsss,
				PhyType.Erp => DOT11_PHY_TYPE.dot11_phy_type_erp,
				PhyType.Ht => DOT11_PHY_TYPE.dot11_phy_type_ht,
				PhyType.Vht => DOT11_PHY_TYPE.dot11_phy_type_vht,
				PhyType.Dmg => DOT11_PHY_TYPE.dot11_phy_type_dmg,
				PhyType.He => DOT11_PHY_TYPE.dot11_phy_type_he,
				PhyType.Eht => DOT11_PHY_TYPE.dot11_phy_type_eht,
				PhyType.IhvStart => DOT11_PHY_TYPE.dot11_phy_type_IHV_start,
				PhyType.IhvEnd => DOT11_PHY_TYPE.dot11_phy_type_IHV_end,
				_ => DOT11_PHY_TYPE.dot11_phy_type_unknown,
			};
		}
	}
}