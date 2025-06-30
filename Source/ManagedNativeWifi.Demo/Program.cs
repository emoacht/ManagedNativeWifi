using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedNativeWifi.Demo;

class Program
{
	static void Main(string[] args)
	{
		if (!Debugger.IsAttached)
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

		ShowInformation();
		Usage.ShowConnectedNetworkInformation();

		//PerformUsage().Wait();

		//ShowRadioInformation();

		//TurnOn();
		//TurnOff();

		//CheckRadioStateEvents();
		//CheckSignalQualityEvents();
	}

	private static void ShowInformation()
	{
		Trace.WriteLine("===== Usable Interfaces =====");
		foreach (var interfaceInfo in NativeWifi.EnumerateInterfaces())
		{
			Trace.WriteLine($"Interface: {interfaceInfo.Description} ({interfaceInfo.Id})");
			Trace.WriteLine($"State: {interfaceInfo.State}");
		}

		Trace.WriteLine("===== Usable Interface Connections =====");
		foreach (var interfaceConnectionInfo in NativeWifi.EnumerateInterfaceConnections())
		{
			Trace.WriteLine($"{{Interface: {interfaceConnectionInfo.Description} ({interfaceConnectionInfo.Id})");
			Trace.WriteLine($" State: {interfaceConnectionInfo.State}");
			Trace.WriteLine($" RadioOn: {interfaceConnectionInfo.IsRadioOn}");
			Trace.WriteLine($" Connected: {interfaceConnectionInfo.IsConnected}}}");

			if (interfaceConnectionInfo.IsConnected)
			{
				Trace.WriteLine($" --- Connection ---");
				var (result, cc) = NativeWifi.GetCurrentConnection(interfaceConnectionInfo.Id);
				if (result is ActionResult.Success)
				{
					Trace.WriteLine($"\t{{Mode: {cc.ConnectionMode}");
					Trace.WriteLine($"\t Profile: {cc.ProfileName}");
					Trace.WriteLine($"\t SSID: {cc.Ssid}");
					Trace.WriteLine($"\t BssType: {cc.BssType}");
					Trace.WriteLine($"\t BSSID: {cc.Bssid}");
					Trace.WriteLine($"\t PhyType: {cc.PhyType} 802.11{cc.PhyType.ToProtocolName()}");
					Trace.WriteLine($"\t PhyIndex: {cc.PhyIndex}");
					Trace.WriteLine($"\t SignalQuality: {cc.SignalQuality}");
					Trace.WriteLine($"\t RxRate: {cc.RxRate}");
					Trace.WriteLine($"\t TxRate: {cc.TxRate}");
					Trace.WriteLine($"\t SecurityEnabled: {cc.IsSecurityEnabled}");
					Trace.WriteLine($"\t OneXEnabled: {cc.IsOneXEnabled}");
					Trace.WriteLine($"\t AuthenticationAlgorithm: {cc.AuthenticationAlgorithm}");
					Trace.WriteLine($"\t CipherAlgorithm: {cc.CipherAlgorithm}}}");
				}

				Trace.WriteLine($" ---- RSSI ----");
				(result, int rssi) = NativeWifi.GetRssi(interfaceConnectionInfo.Id);
				if (result is ActionResult.Success)
				{
					Trace.WriteLine($"\t RSSI: {rssi}");
				}

				Trace.WriteLine($" ---- Connection quality ----");
				(result, var rcq) = NativeWifi.GetRealtimeConnectionQuality(interfaceConnectionInfo.Id);
				if (result is ActionResult.Success)
				{
					Trace.WriteLine($"\t{{PhyType: {rcq.PhyType} 802.11{rcq.PhyType.ToProtocolName()}");
					Trace.WriteLine($"\t LinkQuality: {rcq.LinkQuality}");
					Trace.WriteLine($"\t RxRate: {rcq.RxRate} Kbps");
					Trace.WriteLine($"\t TxRate: {rcq.TxRate} Kbps");
					Trace.WriteLine($"\t IsMultiLinkOperation: {rcq.IsMultiLinkOperation}");
					Trace.WriteLine($"\t Links count: {rcq.Links.Count}}}");

					if (rcq.Links.Count > 0)
					{
						foreach (var link in rcq.Links)
						{
							Trace.WriteLine($"\t\t{{LinkId: {link.LinkId}");
							Trace.WriteLine($"\t\t RSSI: {link.Rssi}");
							Trace.WriteLine($"\t\t Frequency: {link.Frequency} MHz");
							Trace.WriteLine($"\t\t Bandwidth: {link.Bandwidth} MHz}}");
						}
					}
				}
			}
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
			Trace.WriteLine($"{{Interface: {network.InterfaceInfo.Description} ({network.InterfaceInfo.Id})");
			Trace.WriteLine($" SSID: {network.Ssid}");
			Trace.WriteLine($" BssType: {network.BssType}");
			Trace.WriteLine($" Connectable: {network.IsConnectable}");
			Trace.WriteLine($" SignalQuality: {network.SignalQuality}");
			Trace.WriteLine($" SecurityEnabled: {network.IsSecurityEnabled}}}");
		}

		Trace.WriteLine("===== Available Network Groups =====");
		foreach (var network in NativeWifi.EnumerateAvailableNetworkGroups())
		{
			Trace.WriteLine($"{{Interface: {network.InterfaceInfo.Description} ({network.InterfaceInfo.Id})");
			Trace.WriteLine($" SSID: {network.Ssid}");
			Trace.WriteLine($" BssNetworks: {network.BssNetworks.Count}");
			Trace.WriteLine($" SignalQuality: {network.SignalQuality}");
			Trace.WriteLine($" LinkQuality: {network.LinkQuality}");
			Trace.WriteLine($" Frequency: {network.Frequency} KHz");
			Trace.WriteLine($" Band: {network.Band} GHz");
			Trace.WriteLine($" Channel: {network.Channel}}}");
		}

		Trace.WriteLine("===== BSS Networks =====");
		foreach (var network in NativeWifi.EnumerateBssNetworks())
		{
			Trace.WriteLine($"{{Interface: {network.InterfaceInfo.Description} ({network.InterfaceInfo.Id})");
			Trace.WriteLine($" SSID: {network.Ssid}");
			Trace.WriteLine($" BssType: {network.BssType}");
			Trace.WriteLine($" BSSID: {network.Bssid}");
			Trace.WriteLine($" PhyType: {network.PhyType} 802.11{network.PhyType.ToProtocolName()}");
			Trace.WriteLine($" RSSI: {network.Rssi}");
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
			Trace.WriteLine($" Interface: {profile.InterfaceInfo.Description} ({profile.InterfaceInfo.Id})");
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
			Trace.WriteLine($" Interface: {profile.InterfaceInfo.Description} ({profile.InterfaceInfo.Id})");
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

			var radioInfo = NativeWifi.GetRadio(interfaceInfo.Id);
			if (radioInfo is null)
				continue;

			foreach (var radioState in radioInfo.RadioStates)
			{
				Trace.WriteLine($"PhyType: {radioState.PhyType}");
				Trace.WriteLine($"HardwareOn: {radioState.IsHardwareOn}, SoftwareOn: {radioState.IsSoftwareOn}, On: {radioState.IsOn}");
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
				Trace.WriteLine($"Turn on: {NativeWifi.TurnOnRadio(interfaceInfo.Id)}");
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
				Trace.WriteLine($"Turn off: {NativeWifi.TurnOffRadio(interfaceInfo.Id)}");
			}
			catch (UnauthorizedAccessException)
			{
				Trace.WriteLine("Turn off: Unauthorized");
			}
		}
	}

	private static void CheckRadioStateEvents()
	{
		using var player = new NativeWifiPlayer();
		player.RadioStateChanged += OnRadioStateChanged;
		Console.WriteLine("Listening RadioStateChanged events. To stop listening, hit any key.");
		Console.ReadKey();
		player.RadioStateChanged -= OnRadioStateChanged;

		static void OnRadioStateChanged(object sender, RadioStateChangedEventArgs e)
		{
			Trace.WriteLine($"{{Interface: ({e.InterfaceId})");
			Trace.WriteLine($" PhyType: {e.RadioState.PhyType}");
			Trace.WriteLine($" HardwareOn: {e.RadioState.IsHardwareOn}");
			Trace.WriteLine($" SoftwareOn: {e.RadioState.IsSoftwareOn}}}");
		}
	}

	private static void CheckSignalQualityEvents()
	{
		using var player = new NativeWifiPlayer();
		player.SignalQualityChanged += OnSignalQualityChanged;
		Console.WriteLine("Listening SignalQualityChanged events. To stop listening, hit any key.");
		Console.ReadKey();
		player.SignalQualityChanged -= OnSignalQualityChanged;

		static void OnSignalQualityChanged(object sender, SignalQualityChangedEventArgs e)
		{
			Trace.WriteLine($"{{Interface: ({e.InterfaceId})");
			Trace.WriteLine($" SignalQuality: {e.SignalQuality}}}");

			var (result, rssi) = NativeWifi.GetRssi(e.InterfaceId);
			if (result is ActionResult.Success)
			{
				Trace.WriteLine($"{{RSSI: {rssi}}}");
			}

			(result, var realtimeConnectionQuality) = NativeWifi.GetRealtimeConnectionQuality(e.InterfaceId);
			if (result is ActionResult.Success)
			{
				Trace.WriteLine($"{{LinkQuality: {realtimeConnectionQuality.LinkQuality}");
				Trace.WriteLine($" RSSI: {realtimeConnectionQuality.Links.FirstOrDefault()?.Rssi}}}");
			}
		}
	}
}