using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using ManagedNativeWifi.Win32;
using static ManagedNativeWifi.Win32.NativeMethod;


namespace ManagedNativeWifi
{
    public class ConnectionAttributePack
    {
	    public InterfaceState IsState;
	    public ConnectionMode ConnectionMode;
		/// <summary>
        /// Associated Profile Name
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
