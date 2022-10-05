using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi.Demo;

class Program
{
	static void Main(string[] args)
	{
		if (!Debugger.IsAttached)
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

		using var wifi = new NativeWifiPlayer();
		wifi.AvailabilityChanged += Wifi_AvailabilityChanged;
		wifi.ConnectionChanged += Wifi_ConnectionChanged;
		wifi.InterfaceChanged += Wifi_InterfaceChanged;
		wifi.NetworkRefreshed += Wifi_NetworkRefreshed;
		wifi.ProfileChanged += Wifi_ProfileChanged;

		ShowInformation();

		//PerformUsage().Wait();

		//ShowRadioInformation();

		//TurnOn();
		//TurnOff();

		Console.ReadKey();

		wifi.AvailabilityChanged -= Wifi_AvailabilityChanged;
		wifi.ConnectionChanged -= Wifi_ConnectionChanged;
		wifi.InterfaceChanged -= Wifi_InterfaceChanged;
		wifi.NetworkRefreshed -= Wifi_NetworkRefreshed;
		wifi.ProfileChanged -= Wifi_ProfileChanged;
	}

	private static void Wifi_ProfileChanged(object sender, Args.ProfileArgs e)
	{
		Console.WriteLine($"{e.Interface.Description} - Profile Changed - {e.State}");
	}

	private static void Wifi_NetworkRefreshed(object sender, EventArgs e)
	{
		Console.WriteLine("Network Refreshed.");
	}

	private static void Wifi_InterfaceChanged(object sender, Args.InterfaceArgs e)
	{
		Console.WriteLine($"{e.Interface.Description} - Interface Changed - {e.State}");
	}

	private static void Wifi_ConnectionChanged(object sender, Args.ConnectionArgs e)
	{
		Console.WriteLine($"{e.Interface.Description} - Connection Changed - {e.State}");
	}

	private static void Wifi_AvailabilityChanged(object sender, Args.AvailabilityArgs e)
	{
		Console.WriteLine($"{e.Interface.Description} - Availability Changed - {e.State}");
	}

	private static void ShowInformation()
	{
		Trace.WriteLine("===== Usable Interfaces =====");
		foreach (var interfaceInfo in NativeWifi.EnumerateInterfaces())
		{
			Trace.WriteLine($"Interface: {interfaceInfo.Description} ({interfaceInfo.Id})");
		}

		Trace.WriteLine("===== Usable Interface Connections =====");
		foreach (var interfaceInfo in NativeWifi.EnumerateInterfaceConnections())
		{
			Trace.WriteLine($"{{Interface: {interfaceInfo.Description} ({interfaceInfo.Id})");
			Trace.WriteLine($" Connection: {interfaceInfo.ConnectionMode}");
			Trace.WriteLine($" RadioOn: {interfaceInfo.IsRadioOn}");
			Trace.WriteLine($" Connected: {interfaceInfo.IsConnected}");
			Trace.WriteLine($" Profile: {interfaceInfo.ProfileName}}}");
		}

		Trace.WriteLine("===== Available Network SSIDs =====");
		foreach (var ssid in NativeWifi.EnumerateAvailableNetworkSsids())
		{
			Trace.WriteLine($"SSID: {ssid}");
		}

		Trace.WriteLine("===== Connected Network SSIDs =====");
		foreach (var ssid in NativeWifi.EnumerateConnectedNetworkSsids())
		{
			Trace.WriteLine($"SSID: {ssid}");
		}

		Trace.WriteLine("===== Available Networks =====");
		foreach (var network in NativeWifi.EnumerateAvailableNetworks())
		{
			Trace.WriteLine($"{{Interface: {network.Interface.Description} ({network.Interface.Id})");
			Trace.WriteLine($" SSID: {network.Ssid}");
			Trace.WriteLine($" BssType: {network.BssType}");
			Trace.WriteLine($" SignalQuality: {network.SignalQuality}");
			Trace.WriteLine($" Security: {network.IsSecurityEnabled}}}");
		}

		Trace.WriteLine("===== Available Network Groups =====");
		foreach (var network in NativeWifi.EnumerateAvailableNetworkGroups())
		{
			Trace.WriteLine($"{{Interface: {network.Interface.Description} ({network.Interface.Id})");
			Trace.WriteLine($" SSID: {network.Ssid}");
			Trace.WriteLine($" BssNetworks: {network.BssNetworks.Count}");
			Trace.WriteLine($" SignalQuality: {network.SignalQuality}");
			Trace.WriteLine($" LinkQuality: {network.LinkQuality}");
			Trace.WriteLine($" Band: {network.Band} GHz");
			Trace.WriteLine($" Channel: {network.Channel}}}");
		}

		Trace.WriteLine("===== BSS Networks =====");
		foreach (var network in NativeWifi.EnumerateBssNetworks())
		{
			Trace.WriteLine($"{{Interface: {network.Interface.Description} ({network.Interface.Id})");
			Trace.WriteLine($" SSID: {network.Ssid}");
			Trace.WriteLine($" BssType: {network.BssType}");
			Trace.WriteLine($" BSSID: {network.Bssid}");
			Trace.WriteLine($" PhyType: {network.PhyType} 802.11{network.PhyType.ToProtocolName()}");
			Trace.WriteLine($" SignalStrength: {network.SignalStrength}");
			Trace.WriteLine($" LinkQuality: {network.LinkQuality}");
			Trace.WriteLine($" Frequency: {network.Frequency} KHz");
			Trace.WriteLine($" Band: {network.Band} GHz");
			Trace.WriteLine($" Channel: {network.Channel}}}");
		}

		Trace.WriteLine("===== Network Profile Names =====");
		foreach (var name in NativeWifi.EnumerateProfileNames())
		{
			Trace.WriteLine($"Name: {name}");
		}

		Trace.WriteLine("===== Network Profiles =====");
		foreach (var profile in NativeWifi.EnumerateProfiles())
		{
			Trace.WriteLine($"{{Name: {profile.Name}");
			Trace.WriteLine($" Interface: {profile.Interface.Description} ({profile.Interface.Id})");
			Trace.WriteLine($" SSID: {profile.Document.Ssid}");
			Trace.WriteLine($" BssType: {profile.Document.BssType}");
			Trace.WriteLine($" Authentication: {profile.Document.Authentication}");
			Trace.WriteLine($" Encryption: {profile.Document.Encryption}");
			Trace.WriteLine($" AutoConnect: {profile.Document.IsAutoConnectEnabled}");
			Trace.WriteLine($" AutoSwitch: {profile.Document.IsAutoSwitchEnabled}");
			Trace.WriteLine($" Position: {profile.Position}}}");
		}

		Trace.WriteLine("===== Network Profile Radios =====");
		foreach (var profile in NativeWifi.EnumerateProfileRadios())
		{
			Trace.WriteLine($"{{Name: {profile.Name}");
			Trace.WriteLine($" Interface: {profile.Interface.Description} ({profile.Interface.Id})");
			Trace.WriteLine($" SSID: {profile.Document.Ssid}");
			Trace.WriteLine($" RadioOn: {profile.IsRadioOn}");
			Trace.WriteLine($" Connected: {profile.IsConnected}");
			Trace.WriteLine($" SignalQuality: {profile.SignalQuality}");
			Trace.WriteLine($" LinkQuality: {profile.LinkQuality}");
			Trace.WriteLine($" Band: {profile.Band} GHz");
			Trace.WriteLine($" Channel: {profile.Channel}}}");
		}
	}

	private static async Task PerformUsage()
	{
		Trace.WriteLine($"Turn on: {await Usage.TurnOnAsync()}");

		foreach (var ssid in Usage.EnumerateNetworkSsids())
			Trace.WriteLine($"Ssid: {ssid}");

		Trace.WriteLine($"Change automatic connection: {Usage.ChangeProfile(true, false)}");
		Trace.WriteLine($"Connect: {await Usage.ConnectAsync()}");

		await Usage.RefreshAsync();

		Trace.WriteLine($"Delete: {Usage.DeleteProfile("TestProfile")}");

		foreach (var channel in Usage.EnumerateNetworkChannels(-60))
			Trace.WriteLine($"Channel: {channel}");
	}

	private static void ShowRadioInformation()
	{
		foreach (var interfaceInfo in NativeWifi.EnumerateInterfaces())
		{
			Trace.WriteLine($"Interface: {interfaceInfo.Description} ({interfaceInfo.Id})");

			var interfaceRadio = NativeWifi.GetInterfaceRadio(interfaceInfo.Id);
			if (interfaceRadio is null)
				continue;

			foreach (var radioSet in interfaceRadio.RadioSets)
			{
				Trace.WriteLine($"Type: {radioSet.Type}");
				Trace.WriteLine($"HardwareOn: {radioSet.HardwareOn}, SoftwareOn: {radioSet.SoftwareOn}, On: {radioSet.On}");
			}
		}
	}

	private static void TurnOn()
	{
		foreach (var interfaceInfo in NativeWifi.EnumerateInterfaces())
		{
			Trace.WriteLine($"Interface: {interfaceInfo.Description} ({interfaceInfo.Id})");

			try
			{
				Trace.WriteLine($"Turn on: {NativeWifi.TurnOnInterfaceRadio(interfaceInfo.Id)}");
			}
			catch (UnauthorizedAccessException)
			{
				Trace.WriteLine("Turn on: Unauthorized");
			}
		}
	}

	private static void TurnOff()
	{
		foreach (var interfaceInfo in NativeWifi.EnumerateInterfaces())
		{
			Trace.WriteLine($"Interface: {interfaceInfo.Description} ({interfaceInfo.Id})");

			try
			{
				Trace.WriteLine($"Turn off: {NativeWifi.TurnOffInterfaceRadio(interfaceInfo.Id)}");
			}
			catch (UnauthorizedAccessException)
			{
				Trace.WriteLine("Turn off: Unauthorized");
			}
		}
	}
}