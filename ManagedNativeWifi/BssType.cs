
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
}