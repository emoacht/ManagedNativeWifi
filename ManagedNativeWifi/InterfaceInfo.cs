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

        /// <summary>
        /// Gets the attributes of the current connection.
        /// </summary>
        /// <value>The current connection attributes.</value>
        /// <exception cref="Win32Exception">An exception with code 0x0000139F (The group or resource is not in the correct state to perform the requested operation.) will be thrown if the interface is not connected to a network.</exception>
        public ConnectionAttributePack CurrentConnection
		{
			get
			{	
				var connection = Base.GetConnectionAttributes(this.Client, this.Id);
				return new ConnectionAttributePack(InterfaceStateConverter.Convert(connection.isState), ConnectionModeConverter.Convert(connection.wlanConnectionMode), connection.strProfileName);
            }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="InterfaceInfo"/> is automatically configured.
		/// </summary>
		/// <value><c>true</c> if "autoconf" is enabled; otherwise, <c>false</c>.</value>
        public bool AutoConf
		{
			get
			{
				return Base.GetInterfaceInt(this.Client,this.Id, WLAN_INTF_OPCODE.wlan_intf_opcode_autoconf_enabled) != 0;
			}
			set
			{
				Base.SetIntesrfaceInt(this.Client, this.Id, WLAN_INTF_OPCODE.wlan_intf_opcode_autoconf_enabled, value ? 1 : 0);
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