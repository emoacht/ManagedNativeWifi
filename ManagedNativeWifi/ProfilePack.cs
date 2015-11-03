using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Wireless profile information
	/// </summary>
	public class ProfilePack
	{
		/// <summary>
		/// Profile name
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Associated wireless interface
		/// </summary>
		public InterfaceInfo Interface { get; }

		/// <summary>
		/// Profile type
		/// </summary>
		public ProfileType ProfileType { get; }

		/// <summary>
		/// XML representation of this profile
		/// </summary>
		public string ProfileXml { get; }

		/// <summary>
		/// SSID of associated wireless LAN
		/// </summary>
		public NetworkIdentifier Ssid { get; }

		/// <summary>
		/// BSS network type of associated wireless LAN
		/// </summary>
		public BssType BssType { get; }

		/// <summary>
		/// Authentication type of associated wireless LAN
		/// </summary>
		public string Authentication { get; }

		/// <summary>
		/// Encryption type of associated wireless LAN
		/// </summary>
		public string Encryption { get; }

		/// <summary>
		/// Signal quality of associated wireless LAN
		/// </summary>
		public int SignalQuality { get; }

		/// <summary>
		/// Position in preference order of associated wireless interface
		/// </summary>
		public int Position { get; }

		/// <summary>
		/// Whether this profile is set to be automatically connected
		/// </summary>
		public bool IsAutomatic { get; }

		/// <summary>
		/// Whether this profile is currently connected
		/// </summary>
		public bool IsConnected { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ProfilePack(
			string name,
			InterfaceInfo interfaceInfo,
			ProfileType profileType,
			string profileXml,
			NetworkIdentifier ssid,
			BssType bssType,
			string authentication,
			string encryption,
			int signalQuality,
			int position,
			bool isAutomatic,
			bool isConnected)
		{
			this.Name = name;
			this.Interface = interfaceInfo;
			this.ProfileType = profileType;
			this.ProfileXml = profileXml;
			this.Ssid = ssid;
			this.BssType = bssType;
			this.Authentication = authentication;
			this.Encryption = encryption;
			this.SignalQuality = signalQuality;
			this.Position = position;
			this.IsAutomatic = isAutomatic;
			this.IsConnected = isConnected;
		}
	}
}