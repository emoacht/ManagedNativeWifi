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
		/// Signal quality (0-100)
		/// </summary>
		public int SignalQuality { get; }

		/// <summary>
		/// Whether security is enabled on this network
		/// </summary>
		public bool IsSecurityEnabled { get; }

		/// <summary>
		/// Associated wireless profile name
		/// </summary>
		public string ProfileName { get; }

		public bool IsHasProfile
		{
			get { return !string.IsNullOrEmpty(ProfileName); }
		}

		/// <summary>
        /// 
        /// </summary>
		public AuthType AuthAlgorithm { get; }

		/// <summary>
        /// 
        /// </summary>
		public EncryptionType CipherAlgorithm { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AvailableNetworkPack(
			InterfaceInfo interfaceInfo,
			NetworkIdentifier ssid,
			BssType bssType,
			int signalQuality,
			bool isSecurityEnabled,
			string profileName,
			AuthType authAlgorithm,
			EncryptionType cipherAlgorithm)
		{
			this.Interface = interfaceInfo;
			this.Ssid = ssid;
			this.BssType = bssType;
			this.SignalQuality = signalQuality;
			this.IsSecurityEnabled = isSecurityEnabled;
			this.ProfileName = profileName;
			this.AuthAlgorithm = authAlgorithm;
			this.CipherAlgorithm = cipherAlgorithm;
		}
	}
}