using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Wireless LAN information
	/// </summary>
	public class BssNetworkPack
	{
		/// <summary>
		/// Associated wireless interface information
		/// </summary>
		public InterfaceInfo Interface { get; }

		/// <summary>
		/// SSID (maximum 32 bytes)
		/// </summary>
		public NetworkIdentifier Ssid { get; }

		/// <summary>
		/// BSS network type
		/// </summary>
		public BssType BssType { get; }

		/// <summary>
		/// BSSID (6 bytes)
		/// </summary>
		public NetworkIdentifier Bssid { get; }

		/// <summary>
		/// Link quality (0-100)
		/// </summary>
		public int LinkQuality { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public BssNetworkPack(
			InterfaceInfo interfaceInfo,
			NetworkIdentifier ssid,
			BssType bssType,
			NetworkIdentifier bssid,
			int linkQuality)
		{
			this.Interface = interfaceInfo;
			this.Ssid = ssid;
			this.BssType = bssType;
			this.Bssid = bssid;
			this.LinkQuality = linkQuality;
		}
	}
}