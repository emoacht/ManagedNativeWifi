
namespace ManagedNativeWifi
{
	/// <summary>
	/// 802.11 PHY and media type
	/// </summary>
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
}