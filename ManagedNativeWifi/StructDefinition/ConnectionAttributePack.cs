using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using ManagedNativeWifi.Win32;
using static ManagedNativeWifi.Win32.NativeMethod;


namespace ManagedNativeWifi
{
    /// <summary>
    /// Defines the attributes of a wireless connection. (<see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms706842(v=vs.85).aspx">Document</see>)
    /// </summary>
    /// <see cref="WLAN_CONNECTION_ATTRIBUTES"/>
    public class ConnectionAttributePack
    {
        /// <summary>
        /// A WLAN_INTERFACE_STATE value that indicates the state of the interface.
        /// </summary>
        public InterfaceState IsState;
        /// <summary>
        /// A WLAN_CONNECTION_MODE value that indicates the mode of the connection.
        /// </summary>
        public ConnectionMode ConnectionMode;
        /// <summary>
        /// The name of the profile used for the connection. 
        /// </summary>
        public string ProfileName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isState"></param>
        /// <param name="connectionMode"></param>
        /// <param name="profileName"></param>
        public ConnectionAttributePack(
	        InterfaceState isState,
	        ConnectionMode connectionMode,
		    string profileName)
	    {
		    this.IsState = isState;
		    this.ConnectionMode = connectionMode;
		    this.ProfileName = profileName;
	    }
		
    }
}
