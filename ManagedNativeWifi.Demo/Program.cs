﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi.Demo
{
	class Program
	{
		static void Main(string[] args)
		{
			if (!Debugger.IsAttached)
				Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

			ShowInformation();

			PerformUsage().Wait();

			//ShowRadioInformation();

			//TurnOn();
			//TurnOff();
		}

		private static void ShowInformation()
		{
			Trace.WriteLine("[Usable Interfaces]");
			foreach (var interfaceInfo in NativeWifi.EnumerateInterfaces())
			{
				Trace.WriteLine($"Interface: {interfaceInfo.Description} ({interfaceInfo.Id})");
			}

			Trace.WriteLine("");
            Trace.WriteLine("[Available Network SSIDs]");
			foreach (var ssid in NativeWifi.EnumerateAvailableNetworkSsids())
			{
				Trace.WriteLine($"SSID: {ssid}");
			}

			Trace.WriteLine("");
            Trace.WriteLine("[Connected Network SSIDs]");
			foreach (var ssid in NativeWifi.EnumerateConnectedNetworkSsids())
			{
				Trace.WriteLine($"SSID: {ssid}");
			}

			Trace.WriteLine("");
            Trace.WriteLine("[Available Networks]");
			foreach (var network in NativeWifi.EnumerateAvailableNetworks())
			{
				Trace.WriteLine($"{{Interface: {network.Interface.Description} ({network.Interface.Id})");
				Trace.WriteLine($" SSID: {network.Ssid}");
				Trace.WriteLine($" BSS: {network.BssType}");
				Trace.WriteLine($" Signal: {network.SignalQuality}");
				Trace.WriteLine($" Security: {network.IsSecurityEnabled}");
				Trace.WriteLine($" Auth Algorithm: {network.AuthAlgorithm.ToString()}");
				Trace.WriteLine($" Has Profile: {network.IsHasProfile} {(network.IsHasProfile ? "(" + network.ProfileName + ")" : network.ProfileName)}");

                Trace.WriteLine($" Cipher Algorithm: {network.CipherAlgorithm.ToString()}}}");
				var bssNetworks = NativeWifi.EnumerateBssNetworks(network);
				if (bssNetworks != null)
				{
					Trace.WriteLine("[Child BSS Networks]");
					foreach (var bssNetwork in bssNetworks)
					{
						Trace.WriteLine($"{{BSSID: {bssNetwork.Bssid}");
                        Trace.WriteLine($" SSID: {bssNetwork.Ssid}");
						Trace.WriteLine($" BSS: {bssNetwork.BssType}");
						Trace.WriteLine($" Signal: {bssNetwork.SignalStrength}");
						Trace.WriteLine($" Link: {bssNetwork.LinkQuality}");
						Trace.WriteLine($" Frequency: {bssNetwork.Frequency}");
						Trace.WriteLine($" Channel: {bssNetwork.Channel}}}");
					}
				}
			}

			Trace.WriteLine("");
			Trace.WriteLine("[BSS Networks]");
			foreach (var network in NativeWifi.EnumerateBssNetworks())
			{
				Trace.WriteLine($"{{Interface: {network.Interface.Description} ({network.Interface.Id})");
				Trace.WriteLine($" SSID: {network.Ssid}");
				Trace.WriteLine($" BSS: {network.BssType}");
				Trace.WriteLine($" BSSID: {network.Bssid}");
				Trace.WriteLine($" Signal: {network.SignalStrength}");
				Trace.WriteLine($" Link: {network.LinkQuality}");
				Trace.WriteLine($" Frequency: {network.Frequency}");
				Trace.WriteLine($" Channel: {network.Channel}}}");
			}

			Trace.WriteLine("");
            Trace.WriteLine("[Network Profile Names]");
			foreach (var name in NativeWifi.EnumerateProfileNames())
			{
				Trace.WriteLine($"Name: {name}");
			}

			Trace.WriteLine("");
            Trace.WriteLine("[Network Profiles]");
			foreach (var profile in NativeWifi.EnumerateProfiles())
			{
				Trace.WriteLine($"{{Name: {profile.Name}");
				Trace.WriteLine($" Interface: {profile.Interface.Description} ({profile.Interface.Id})");
				Trace.WriteLine($" SSID: {profile.Document.Ssid}");
				Trace.WriteLine($" BSS: {profile.Document.BssType}");
				Trace.WriteLine($" Authentication: {profile.Document.Authentication}");
				Trace.WriteLine($" Encryption: {profile.Document.Encryption}");
				Trace.WriteLine($" AutoConnect: {profile.Document.IsAutoConnectEnabled}");
				Trace.WriteLine($" AutoSwitch: {profile.Document.IsAutoSwitchEnabled}");
				Trace.WriteLine($" Position: {profile.Position}");
				Trace.WriteLine($" RadioOn: {profile.IsRadioOn}");
				Trace.WriteLine($" Signal: {profile.SignalQuality}");
				Trace.WriteLine($" Connected: {profile.IsConnected}}}");
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
				if (interfaceRadio == null)
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
}