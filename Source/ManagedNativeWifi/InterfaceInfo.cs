using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Wireless interface information
	/// </summary>
	public class InterfaceInfo
	{
		/// <summary>
		/// Interface ID
		/// </summary>
		public Guid Id { get; }

		/// <summary>
		/// Interface description
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Interface state
		/// </summary>
		public InterfaceState State { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public InterfaceInfo(Guid id, string description, InterfaceState state)
		{
			this.Id = id;
			this.Description = description;
			this.State = state;
		}

		internal InterfaceInfo(WLAN_INTERFACE_INFO info)
		{
			Id = info.InterfaceGuid;
			Description = info.strInterfaceDescription;
			State = InterfaceStateConverter.Convert(info.isState);
		}
	}

	/// <summary>
	/// Wireless interface and related connection information
	/// </summary>
	public class InterfaceConnectionInfo : InterfaceInfo
	{
		/// <summary>
		/// Connection mode
		/// </summary>
		public ConnectionMode ConnectionMode { get; }

		/// <summary>
		/// Whether the radio of the wireless interface is on
		/// </summary>
		public bool IsRadioOn { get; }

		/// <summary>
		/// Whether the wireless interface is connected to a wireless LAN
		/// </summary>
		public bool IsConnected { get; }

		/// <summary>
		/// Wireless profile name when the wireless profile is used for the connection
		/// </summary>
		public string ProfileName { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		internal InterfaceConnectionInfo(
			WLAN_INTERFACE_INFO info,
			ConnectionMode connectionMode,
			bool isRadioOn,
			bool isConnected,
			string profileName) : base(info)
		{
			this.ConnectionMode = connectionMode;
			this.IsRadioOn = isRadioOn;
			this.IsConnected = isConnected;
			this.ProfileName = profileName;
		}
	}
}