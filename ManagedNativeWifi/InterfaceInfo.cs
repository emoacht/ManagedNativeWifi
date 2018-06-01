using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ManagedNativeWifi.Win32;
using static ManagedNativeWifi.Win32.NativeMethod;
using Base = ManagedNativeWifi.Win32.BaseMethod;

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

		private SafeClientHandle Client { get; }

		public ConnectionAttributePack CurrentConnection
		{
			get
			{
				var connection = Base.GetConnectionAttributes(this.Client, this.Id);
				return new ConnectionAttributePack(InterfaceStateConverter.Convert(connection.isState), ConnectionModeConverter.Convert(connection.wlanConnectionMode), connection.strProfileName);
            }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		internal InterfaceInfo(SafeClientHandle client, Guid id, string description, InterfaceState state)
		{
			
            this.Id = id;
			this.Description = description;
			this.State = state;
			this.Client = client;
		}

		internal InterfaceInfo(SafeClientHandle client, WLAN_INTERFACE_INFO info)
		{
			Id = info.InterfaceGuid;
			Description = info.strInterfaceDescription;
			State = InterfaceStateConverter.Convert(info.isState);
        }
	}
}