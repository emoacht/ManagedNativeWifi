using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi.Demo
{
	public static class Usage
	{
		/// <summary>
		/// Enumerate SSIDs of available wireless LANs.
		/// </summary>
		/// <returns>SSID strings</returns>
		public static IEnumerable<string> EnumerateNetworkSsids()
		{
			return NativeWifi.EnumerateAvailableNetworkSsids()
				.Select(x => x.ToString()); // UTF-8 string representation
		}

		/// <summary>
		/// Connect to the available wireless LAN which has the highest signal quality.
		/// </summary>
		/// <returns>True if successfully connected. False if not.</returns>
		public static async Task<bool> ConnectAsync()
		{
			var availableNetwork = NativeWifi.EnumerateAvailableNetworks()
				.Where(x => !string.IsNullOrWhiteSpace(x.ProfileName))
				.OrderByDescending(x => x.SignalQuality)
				.FirstOrDefault();

			if (availableNetwork == null)
				return false;

			return await NativeWifi.ConnectNetworkAsync(
				interfaceId: availableNetwork.Interface.Id,
				profileName: availableNetwork.ProfileName,
				bssType: availableNetwork.BssType,
				timeout: TimeSpan.FromSeconds(10));
		}

		/// <summary>
		/// Refresh available wireless LANs.
		/// </summary>
		public static async Task RefreshAsync()
		{
			await NativeWifi.ScanNetworksAsync(timeout: TimeSpan.FromSeconds(10));
		}

		/// <summary>
		/// Delete a specified wireless profile.
		/// </summary>
		/// <param name="profileName">Profile name to be deleted</param>
		/// <returns>True if successfully deleted. False if not.</returns>
		/// <remarks>Profile name is case-sensitive.</remarks>
		public static bool DeleteProfile(string profileName)
		{
			var targetProfile = NativeWifi.EnumerateProfiles()
				.Where(x => profileName.Equals(x.Name, StringComparison.Ordinal))
				.FirstOrDefault();

			if (targetProfile == null)
				return false;

			return NativeWifi.DeleteProfile(
				interfaceId: targetProfile.Interface.Id,
				profileName: profileName);
		}

		/// <summary>
		/// Enumerate wireless LAN channels whose signal strength go beyond a specified threshold.
		/// </summary>
		/// <param name="signalStrengthThreshold">Threshold of signal strength</param>
		/// <returns>Channel numbers</returns>
		public static IEnumerable<int> EnumerateNetworkChannels(int signalStrengthThreshold)
		{
			return NativeWifi.EnumerateBssNetworks()
				.Where(x => x.SignalStrength > signalStrengthThreshold)
				.Select(x => x.Channel);
		}

		/// <summary>
		/// Turn on the radio of a wireless interface which is not currently on but can be on.
		/// </summary>
		/// <returns>True if successfully turned on. False if not.</returns>
		public static async Task<bool> TurnOnAsync()
		{
			var targetInterface = NativeWifi.EnumerateInterfaces()
				.FirstOrDefault(x =>
				{
					var radioSet = NativeWifi.GetInterfaceRadio(x.Id)?.RadioSets.FirstOrDefault();
					if (radioSet == null)
						return false;

					if (!radioSet.HardwareOn.GetValueOrDefault()) // Hardware radio state is off.
						return false;

					return (radioSet.SoftwareOn == false); // Software radio state is off.
				});

			if (targetInterface == null)
				return false;

			try
			{
				return await Task.Run(() => NativeWifi.TurnOnInterfaceRadio(targetInterface.Id));
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
		}
	}
}