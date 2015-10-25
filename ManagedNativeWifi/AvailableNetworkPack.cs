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
	public class AvailableNetworkPack
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
		/// Signal quality (0-100)
		/// </summary>
		public int SignalQuality { get; }

		/// <summary>
		/// Name of associated wireless profile
		/// </summary>
		public string ProfileName { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public AvailableNetworkPack(
			Guid interfaceGuid,
			string interfaceDescription,
			NetworkIdentifier ssid,
			BssType bssType,
			int signalQuality,
			string profileName)
		{
			this.InterfaceGuid = interfaceGuid;
			this.InterfaceDescription = interfaceDescription;
			this.Ssid = ssid;
			this.BssType = bssType;
			this.SignalQuality = signalQuality;
			this.ProfileName = profileName;
		}
	}
}