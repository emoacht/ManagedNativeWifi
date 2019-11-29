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
		[Obsolete("Use Document.Xml method instead.")]
		public string ProfileXml => Document.Xml;

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
		/// Constructor
		/// </summary>
		public ProfilePack(
			string name,
			InterfaceInfo interfaceInfo,
			ProfileType profileType,
			string profileXml,
			int position)
		{
			this.Name = name;
			this.Interface = interfaceInfo;
			this.ProfileType = profileType;
			Document = new ProfileDocument(profileXml);
			this.Position = position;
		}
	}

	/// <summary>
	/// Wireless profile and related radio information
	/// </summary>
	public class ProfileRadioPack : ProfilePack
	{
		/// <summary>
		/// Associated wireless interface and associated information
		/// </summary>
		public new InterfaceConnectionInfo Interface => (InterfaceConnectionInfo)base.Interface;

		/// <summary>
		/// Whether radio of associated wireless interface is on
		/// </summary>
		public bool IsRadioOn => Interface.IsRadioOn;

		/// <summary>
		/// Whether associated wireless interface is connected to associated wireless LAN
		/// </summary>
		public bool IsConnected => Interface.IsConnected
			&& string.Equals(this.Name, Interface.ProfileName, StringComparison.Ordinal);

		/// <summary>
		/// Signal quality of associated wireless LAN
		/// </summary>
		public int SignalQuality { get; }

		/// <summary>
		/// Link quality of associated wireless LAN
		/// </summary>
		public int LinkQuality { get; }

		/// <summary>
		/// Frequency (KHz) of associated wireless LAN
		/// </summary>
		public int Frequency { get; }

		/// <summary>
		/// Frequency band (GHz) of associated wireless LAN
		/// </summary>
		public float Band { get; }

		/// <summary>
		/// Channel of associated wireless LAN
		/// </summary>
		public int Channel { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ProfileRadioPack(
			string name,
			InterfaceConnectionInfo interfaceInfo,
			ProfileType profileType,
			string profileXml,
			int position,
			int signalQuality,
			int linkQuality,
			int frequency,
			float band,
			int channel) : base(
				name: name,
				interfaceInfo: interfaceInfo,
				profileType: profileType,
				profileXml: profileXml,
				position: position)
		{
			this.SignalQuality = signalQuality;
			this.LinkQuality = linkQuality;
			this.Frequency = frequency;
			this.Band = band;
			this.Channel = channel;
		}
	}
}