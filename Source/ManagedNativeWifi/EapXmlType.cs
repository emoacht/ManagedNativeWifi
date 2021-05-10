using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi
{
	/// <summary>
	/// EAP XML profile type
	/// </summary>
	public enum EapXmlType
	{
		/// <summary>
		/// Default value
		/// </summary>
		/// <remarks>No constant in Wlanapi.h, but needed for EAP-TLS; seems to indicate local user only</remarks>
		Default = 0,

		/// <summary>
		/// Set EAP host data for all users of this profile.
		/// </summary>
		/// <remarks>Equivalent to WLAN_SET_EAPHOST_DATA_ALL_USERS</remarks>
		AllUsers = 1,
	}

	internal static class EapXmlTypeConverter
	{
		public static bool TryConvert(uint source, out EapXmlType profileType)
		{
			if (Enum.IsDefined(typeof(EapXmlType), (int)source))
			{
				profileType = (EapXmlType)source;
				return true;
			}
			profileType = default;
			return false;
		}

		public static uint ConvertBack(EapXmlType source) =>
			(uint)source;
	}
}
