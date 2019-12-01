using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// 802.11 PHY and media type
	/// </summary>
	/// <remarks>
	/// Equivalent to DOT11_PHY_TYPE:
	/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/dot11-phy-type
	/// </remarks>
	public enum PhyType
	{
		/// <summary>
		/// Unknown or uninitialized PHY type
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Any PHY type
		/// </summary>
		Any,

		/// <summary>
		/// Frequency-hopping spread-spectrum (FHSS) PHY type
		/// </summary>
		Fhss,

		/// <summary>
		/// Direct sequence spread spectrum (DSSS) PHY type
		/// </summary>
		Dsss,

		/// <summary>
		/// Infrared (IR) baseband PHY type
		/// </summary>
		IrBaseband,

		/// <summary>
		/// Orthogonal frequency division multiplexing (OFDM) PHY type
		/// </summary>
		Ofdm,

		/// <summary>
		/// High-rate DSSS (HRDSSS) PHY type
		/// </summary>
		HrDsss,

		/// <summary>
		/// Extended rate PHY (ERP) type
		/// </summary>
		Erp,

		/// <summary>
		/// 802.11n PHY type
		/// </summary>
		Ht,

		/// <summary>
		/// 802.11ac PHY type
		/// </summary>
		Vht,

		/// <summary>
		/// The start of the range that is used to define PHY types that are developed by
		/// an independent hardware vendor (IHV)
		/// </summary>
		IhvStart,

		/// <summary>
		/// The start of the range that is used to define PHY types that are developed by
		/// an independent hardware vendor (IHV)
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
				PhyType.IhvStart => DOT11_PHY_TYPE.dot11_phy_type_IHV_start,
				PhyType.IhvEnd => DOT11_PHY_TYPE.dot11_phy_type_IHV_end,
				_ => DOT11_PHY_TYPE.dot11_phy_type_unknown,
			};
		}
	}
}