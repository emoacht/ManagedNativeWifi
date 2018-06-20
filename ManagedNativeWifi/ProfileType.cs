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
		GroupPolicy =1,

		/// <summary>
		/// Per-user profile
		/// </summary>
		/// <remarks>Equivalent to WLAN_PROFILE_USER</remarks>
		PerUser =2,

		/// <summary>
		/// On input, this flag indicates that the caller wants to retrieve the plain text key from a wireless profile. If the calling thread has the required permissions, the WlanGetProfile function returns the plain text key in the keyMaterial element of the profile returned in the buffer pointed to by the pstrProfileXml parameter.
		/// Windows 7:  This flag passed on input is an extension to native wireless APIs added on Windows 7 and later. The pdwFlags parameter is an __inout_opt parameter on Windows 7 and later.
		/// </summary>
		GetPlaintextKey = 4
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