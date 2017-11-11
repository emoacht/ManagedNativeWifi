using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

	internal static class ProfileTypeConverter
	{
		public static ProfileType ToProfileType(uint source)
		{
			return Enum.IsDefined(typeof(ProfileType), (int)source)
				? (ProfileType)source
				: throw new ArgumentOutOfRangeException(nameof(source));
		}

		public static uint FromProfileType(ProfileType source) =>
			(uint)source;
	}
}