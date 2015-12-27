Managed Native Wifi
===================

A managed implementation of [Native Wifi][1] API

##Requirements

 * Windows 7 or newer
 * .NET Framework 4.5.2

##Methods

Available methods including asynchronous ones based on TAP.

| Method                         | Description                                                                                       |
|--------------------------------|---------------------------------------------------------------------------------------------------|
| EnumerateInterfaces            | Enumerate wireless interface information.                                                         |
| ScanNetworksAsync              | Asynchronously request wireless interfaces to scan (rescan) wireless LANs.                        |
| EnumerateAvailableNetworkSsids | Enumerate SSIDs of available wireless LANs.                                                       |
| EnumerateConnectedNetworkSsids | Enumerate SSIDs of connected wireless LANs.                                                       |
| EnumerateAvailableNetworks     | Enumerate wireless LAN information on available networks.                                         |
| EnumerateBssNetworks           | Enumerate wireless LAN information on BSS networks.                                               |
| EnumerateProfileNames          | Enumerate wireless profile names in preference order.                                             |
| EnumerateProfiles              | Enumerate wireless profile information in preference order.                                       |
| SetProfile                     | Set (add or overwrite) the content of a specified wireless profile.                               |
| SetProfilePosition             | Set the position of a specified wireless profile in preference order.                             |
| DeleteProfile                  | Delete a specified wireless profile.                                                              |
| ConnectNetwork                 | Attempt to connect to the wireless LAN associated to a specified wireless profile.                |
| ConnectNetworkAsync            | Asynchronously attempt to connect to the wireless LAN associated to a specified wireless profile. |
| DisconnectNetwork              | Disconnect from the wireless LAN associated to a specified wireless interface.                    |
| DisconnectNetworkAsync         | Asynchronously disconnect from the wireless LAN associated to a specified wireless interface.     |
| GetInterfaceRadio              | Get wireless interface radio information of a specified wireless interface.                       |
| TurnOnInterfaceRadio           | Turn on the radio of a specified wireless interface (software radio state only).                  |
| TurnOffInterfaceRadio          | Turn off the radio of a specified wireless interface (software radio state only).                 |

##Usage

To check SSIDs of currently available wireless LANs, call EnumerateAvailableNetworkSsids method.

```csharp
public static IEnumerable<string> EnumerateNetworkSsids()
{
    return NativeWifi.EnumerateAvailableNetworkSsids()
        .Select(x => x.ToString()); // UTF-8 string representation
}
```

In general, a SSID is represented by a UTF-8 string but it is not guaranteed. If the string seems not correct, try ToBytes method instead.

To connect to a wireless LAN, call ConnectNetworkAsync asynchronous method.

```csharp
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
```

This method returns true if successfully connected to the wireless LAN in contrast to its synchronous sibling, ConnectNetwork method, returns true if the request for the connection succeeds and doesn't indicate the result.

To refresh currently available wireless LANs, call ScanNetworksAsync method.

```csharp
public static async Task RefreshAsync()
{
    await NativeWifi.ScanNetworksAsync(timeout: TimeSpan.FromSeconds(10));
}
```

This method requests wireless interfaces to scan wireless LANs in parallel. It takes no more than 4 seconds.

To delete an existing wireless profile, use DeleteProfile method. Please note that a profile name is case-sensitive.

```csharp
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
```

To check wireless LAN channels that are already used by surrounding access points, call EnumerateBssNetworks method and filter the results by signal strength.

```csharp
public static IEnumerable<int> EnumerateNetworkChannels(int signalStrengthThreshold)
{
    return NativeWifi.EnumerateBssNetworks()
        .Where(x => x.SignalStrength > signalStrengthThreshold)
        .Select(x => x.Channel);
}
```

To turn on the radio of a wireless interface, check the current radio state by GetInterfaceRadio method and then call TurnOnInterfaceRadio method.

Please note that this can only change software radio state and if hardware radio state is off (like hardware Wi-Fi switch is turned off), it cannot be changed.

```csharp
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
```

##License

 - MIT License

[1]: https://msdn.microsoft.com/en-us/library/windows/desktop/ms706556.aspx
