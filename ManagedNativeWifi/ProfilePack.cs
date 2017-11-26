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
		/// Associated wireless interface information
		/// </summary>
		public InterfaceInfo Interface { get; }

		/// <summary>
		/// Profile type
		/// </summary>
		public ProfileType ProfileType { get; }

		/// <summary>
		/// Profile XML document
		/// </summary>
		public ProfileDocument Document { get; }

		/// <summary>
		/// Profile XML string
		/// </summary>
		[Obsolete("Use Document.ToString method instead.")]
		public string ProfileXml => Document.ToString();

		/// <summary>
		/// SSID of associated wireless LAN
		/// </summary>
		[Obsolete("Use Document.Ssid property instead.")]
		public NetworkIdentifier Ssid => Document.Ssid;

		/// <summary>
		/// BSS network type of associated wireless LAN
		/// </summary>
		[Obsolete("Use Document.BssType property instead.")]
		public BssType BssType => Document.BssType;

		/// <summary>
		/// Authentication of associated wireless LAN
		/// </summary>
		[Obsolete("Use Document.Authentication property instead.")]
		public string Authentication => Document.AuthenticationString;

		/// <summary>
		/// Encryption of associated wireless LAN
		/// </summary>
		[Obsolete("Use Document.Encryption property instead.")]
		public string Encryption => Document.EncryptionString;

		/// <summary>
		/// Whether this profile is set to be automatically connected
		/// </summary>
		[Obsolete("Use Document.IsAutoConnectEnabled property instead.")]
		public bool IsAutomatic => Document.IsAutoConnectEnabled;

		/// <summary>
		/// Position in preference order of associated wireless interface
		/// </summary>
		public int Position { get; }

		/// <summary>
		/// Signal quality of associated wireless LAN
		/// </summary>
		public int SignalQuality { get; }

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
			int position,
			int signalQuality,
			bool isConnected)
		{
			this.Name = name;
			this.Interface = interfaceInfo;
			this.ProfileType = profileType;
			Document = new ProfileDocument(profileXml);
			this.Position = position;
			this.SignalQuality = signalQuality;
			this.IsConnected = isConnected;
		}
	}
}