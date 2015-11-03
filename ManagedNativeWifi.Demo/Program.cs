using System;
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

			Trace.WriteLine("[Usable Interfaces]");
			foreach (var interfaceInfo in NativeWifi.EnumerateInterfaces())
			{
				Trace.WriteLine($"Interface: {interfaceInfo.Description} ({interfaceInfo.Id})");
			}

			Trace.WriteLine("[Available Network SSIDs]");
			foreach (var ssid in NativeWifi.EnumerateAvailableNetworkSsids())
			{
				Trace.WriteLine($"SSID: {ssid}");
			}

			Trace.WriteLine("[Connected Network SSIDs]");
			foreach (var ssid in NativeWifi.EnumerateConnectedNetworkSsids())
			{
				Trace.WriteLine($"SSID: {ssid}");
			}

			Trace.WriteLine("[Available Networks]");
			foreach (var network in NativeWifi.EnumerateAvailableNetworks())
			{
				Trace.WriteLine($"{{Interface: {network.Interface.Description} ({network.Interface.Id})");
				Trace.WriteLine($" SSID: {network.Ssid}");
				Trace.WriteLine($" BSS network type: {network.BssType}");
				Trace.WriteLine($" Signal quality: {network.SignalQuality}}}");
			}

			Trace.WriteLine("[BSS Networks]");
			foreach (var network in NativeWifi.EnumerateBssNetworks())
			{
				Trace.WriteLine($"{{Interface: {network.Interface.Description} ({network.Interface.Id})");
				Trace.WriteLine($" SSID: {network.Ssid}");
				Trace.WriteLine($" BSS network type: {network.BssType}");
				Trace.WriteLine($" BSSID: {network.Bssid}");
				Trace.WriteLine($" Link quality: {network.LinkQuality}}}");
			}

			Trace.WriteLine("[Network Profile Names]");
			foreach (var name in NativeWifi.EnumerateProfileNames())
			{
				Trace.WriteLine($"Name: {name}");
			}

			Trace.WriteLine("[Network Profiles]");
			foreach (var profile in NativeWifi.EnumerateProfiles())
			{
				Trace.WriteLine($"{{Name: {profile.Name}");
				Trace.WriteLine($" Interface: {profile.Interface.Description} ({profile.Interface.Id})");
				Trace.WriteLine($" SSID: {profile.Ssid}");
				Trace.WriteLine($" BSS network type: {profile.BssType}");
				Trace.WriteLine($" Authentication: {profile.Authentication}");
				Trace.WriteLine($" Encryption: {profile.Encryption}");
				Trace.WriteLine($" Signal quality: {profile.SignalQuality}");
				Trace.WriteLine($" Position: {profile.Position}");
				Trace.WriteLine($" Automatic: {profile.IsAutomatic}");
				Trace.WriteLine($" Connected: {profile.IsConnected}}}");
			}
		}
	}
}