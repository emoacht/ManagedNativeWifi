using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ManagedNativeWifi.Win32.NativeMethod;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Wireless interface state
	/// </summary>
	/// <remarks>Equivalent to WLAN_INTERFACE_STATE</remarks>
	public enum InterfaceState
	{
		/// <summary>
		/// The interface is not ready to operate.
		/// </summary>
		NotReady = 0,

		/// <summary>
		/// The interface is connected to a network.
		/// </summary>
		Connected,

		/// <summary>
		/// The interface is the first node in an ad hoc network. No peer has connected.
		/// </summary>
		AdHocNetworkFormed,

		/// <summary>
		/// The interface is disconnecting from the current network.
		/// </summary>
		Disconnecting,

		/// <summary>
		/// The interface is not connected to any network.
		/// </summary>
		Disconnected,

		/// <summary>
		/// The interface is attempting to associate with a network.
		/// </summary>
		Associating,

		/// <summary>
		/// Auto configuration is discovering the settings for the network.
		/// </summary>
		Discovering,

		/// <summary>
		/// The interface is in the process of authenticating.
		/// </summary>
		Authenticating
	}

	internal static class InterfaceStateConverter
	{
		public static InterfaceState Convert(WLAN_INTERFACE_STATE source) =>
			(InterfaceState)source;

		public static WLAN_INTERFACE_STATE ConvertBack(InterfaceState source) =>
			(WLAN_INTERFACE_STATE)source;
	}
}