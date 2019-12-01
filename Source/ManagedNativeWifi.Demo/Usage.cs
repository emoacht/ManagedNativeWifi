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
		/// Enumerates SSIDs of available wireless LANs.
		/// </summary>
		/// <returns>SSID strings</returns>
		public static IEnumerable<string> EnumerateNetworkSsids()
		{
			return NativeWifi.EnumerateAvailableNetworkSsids()
				.Select(x => x.ToString()); // UTF-8 string representation
		}

		/// <summary>
		/// Connects to the available wireless LAN which has the highest signal quality.
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
		/// Refreshes available wireless LANs.
		/// </summary>
		public static Task RefreshAsync()
		{
			return NativeWifi.ScanNetworksAsync(timeout: TimeSpan.FromSeconds(10));
		}

		/// <summary>
		/// Deletes a specified wireless profile.
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
		/// Enumerates wireless LAN channels whose signal strength go beyond a specified threshold.
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
		/// Turns on the radio of a wireless interface which is not currently on but can be on.
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

		/// <summary>
		/// Changes the automatic connection and automatic switch elements of a wireless profile.
		/// </summary>
		/// <param name="enableAutoConnect">Whether automatic connection should be enabled</param>
		/// <param name="enableAutoSwitch">Whether automatic switch should be enabled</param>
		/// <returns>True if successfully changed. False if not.</returns>
		/// <remarks>A wireless profile made by group policy will be skipped.</remarks>
		public static bool ChangeProfile(bool enableAutoConnect, bool enableAutoSwitch)
		{
			var targetProfile = NativeWifi.EnumerateProfiles()
				.FirstOrDefault(x => x.ProfileType != ProfileType.GroupPolicy);

			if (targetProfile == null)
				return false;

			if ((targetProfile.Document.IsAutoConnectEnabled == enableAutoConnect) &&
				(targetProfile.Document.IsAutoSwitchEnabled == enableAutoSwitch))
				return false;

			// Set IsAutoConnectEnabled first.
			targetProfile.Document.IsAutoConnectEnabled = enableAutoConnect;
			targetProfile.Document.IsAutoSwitchEnabled = enableAutoSwitch;

			return NativeWifi.SetProfile(
				interfaceId: targetProfile.Interface.Id,
				profileType: targetProfile.ProfileType,
				profileXml: targetProfile.Document.Xml,
				profileSecurity: null, // No change
				overwrite: true);
		}
	}
}