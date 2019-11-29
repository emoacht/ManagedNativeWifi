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
		public static bool TryConvert(uint source, out ProfileType profileType)
		{
			if (Enum.IsDefined(typeof(ProfileType), (int)source))
			{
				profileType = (ProfileType)source;
				return true;
			}
			profileType = default(ProfileType);
			return false;
		}

		public static uint ConvertBack(ProfileType source) =>
			(uint)source;
	}
}