
namespace ManagedNativeWifi
{
	/// <summary>
	/// Wireless profile type
	/// </summary>
	public enum ProfileType
	{
		/// <summary>
		/// All-user profile
		/// </summary>
		AllUser = 0,

		/// <summary>
		/// Group policy profile
		/// </summary>
		/// <remarks>Equivalent to WLAN_PROFILE_GROUP_POLICY</remarks>
		GroupPolicy,

		/// <summary>
		/// Per-user profile
		/// </summary>
		/// <remarks>Equivalent to WLAN_PROFILE_USER</remarks>
		PerUser
	}
}
