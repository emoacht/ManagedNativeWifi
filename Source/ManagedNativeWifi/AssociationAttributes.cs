using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Represents WLAN_ASSOCIATION_ATTRIBUTES.
	/// </summary>
	public class AssociationAttributes
	{
		private readonly string _ssid;
		private readonly string _bssType;
		private readonly string _bssid;
		private readonly string _phyType;
		private readonly uint _phyIndex;
		private readonly uint _signalQuality;
		private readonly uint _rxRate;
		private readonly uint _txRate;

		/// <summary>
		/// Represents the attributes of a WLAN association.
		/// </summary>
		/// <param name="ssid">The SSID of the network.</param>
		/// <param name="bssType">The BSS type of the network.</param>
		/// <param name="bssid">The BSSID of the network.</param>
		/// <param name="phyType">The PHY type of the network.</param>
		/// <param name="phyIndex">The PHY index of the network.</param>
		/// <param name="signalQuality">The signal quality of the network.</param>
		/// <param name="rxRate">The receive rate of the network.</param>
		/// <param name="txRate">The transmit rate of the network.</param>
		public AssociationAttributes(string ssid, string bssType, string bssid, string phyType, uint phyIndex, uint signalQuality, uint rxRate, uint txRate)
		{
			this._ssid = ssid;
			this._bssType = bssType;
			this._bssid = bssid;
			this._phyType = phyType;
			this._phyIndex = phyIndex;
			this._signalQuality = signalQuality;
			this._rxRate = rxRate;
			this._txRate = txRate;
		}

		/// <summary>
		/// Gets the SSID of the network.
		/// </summary>
		public string Ssid
		{
			get { return _ssid; }
		}

		/// <summary>
		/// Gets the type of the basic service set (BSS).
		/// </summary>
		public string BssType
		{
			get { return _bssType; }
		}

		/// <summary>
		/// Gets the basic service set identifier (BSSID).
		/// </summary>
		public string Bssid
		{
			get { return _bssid; }
		}

		/// <summary>
		/// Gets the type of the physical layer (PHY).
		/// </summary>
		public string PhyType
		{
			get { return _phyType; }
		}

		/// <summary>
		/// Gets the index of the physical layer (PHY).
		/// </summary>
		public uint PhyIndex
		{
			get { return _phyIndex; }
		}

		/// <summary>
		/// Gets the signal quality.
		/// </summary>
		public uint SignalQuality
		{
			get { return _signalQuality; }
		}

		/// <summary>
		/// Gets the receive rate.
		/// </summary>
		public uint RxRate
		{
			get { return _rxRate; }
		}

		/// <summary>
		/// Gets the transmit rate.
		/// </summary>
		public uint TxRate
		{
			get { return _txRate; }
		}
	}
}