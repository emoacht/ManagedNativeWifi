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
	/// Wireless interface information (Extended)
	/// </summary>
	public class InterfaceInfoExtended : InterfaceInfo
	{
		/// <summary>
		/// Connection mode
		/// </summary>
		public ConnectionMode ConnectionMode { get; }

		/// <summary>
		/// The name of a wireless profile used for the connection, if connected
		/// </summary>
		public string ProfileName { get; }

		/// <summary>
		/// Whether automatic configuration service is enabled
		/// </summary>
		public bool IsAutoConfigEnabled { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		internal InterfaceInfoExtended(WLAN_INTERFACE_INFO info, ConnectionMode connectionMode, string profileName, bool isAutoConfigEnabled)
			: base(info)
		{
			this.ConnectionMode = connectionMode;
			this.ProfileName = profileName;
			this.IsAutoConfigEnabled = isAutoConfigEnabled;
		}
	}
}