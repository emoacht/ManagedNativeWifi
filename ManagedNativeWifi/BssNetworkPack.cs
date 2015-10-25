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
		/// GUID of associated wireless interface
		/// </summary>
		public Guid InterfaceGuid { get; }

		/// <summary>
		/// Description of associated wireless interface
		/// </summary>
		public string InterfaceDescription { get; }

		/// <summary>
		/// SSID (maximum 32 bytes)
		/// </summary>
		public NetworkIdentifier Ssid { get; }

		/// <summary>
		/// BSS type
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
			Guid interfaceGuid,
			string interfaceDescription,
			NetworkIdentifier ssid,
			BssType bssType,
			NetworkIdentifier bssid,
			int linkQuality)
		{
			this.InterfaceGuid = interfaceGuid;
			this.InterfaceDescription = interfaceDescription;
			this.Ssid = ssid;
			this.BssType = bssType;
			this.Bssid = bssid;
			this.LinkQuality = linkQuality;
		}
	}
}