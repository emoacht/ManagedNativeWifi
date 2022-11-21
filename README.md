# Managed Native Wifi

ManagedNativeWifi is a managed implementation of [Native Wifi][1] API. It provides functionality to manage wireless networks, interfaces and profiles.

## Requirements

This library works on Windows and compatible with:

.NET 6.0|.NET Standard 2.0 (including .NET Framework 4.6.1)
-|-

## Download

NuGet: [ManagedNativeWifi][2]

## Methods

Available methods including asynchronous ones based on TAP.

| Method                          | Description                                                                                        |
|---------------------------------|----------------------------------------------------------------------------------------------------|
| EnumerateInterfaces             | Enumerates wireless interface information.                                                         |
| EnumerateInterfaceConnections   | Enumerates wireless interface and related connection information.                                  |
| ScanNetworksAsync               | Asynchronously requests wireless interfaces to scan (rescan) wireless LANs.                        |
| EnumerateAvailableNetworkSsids  | Enumerates SSIDs of available wireless LANs.                                                       |
| EnumerateConnectedNetworkSsids  | Enumerates SSIDs of connected wireless LANs.                                                       |
| EnumerateAvailableNetworks      | Enumerates wireless LAN information on available networks.                                         |
| EnumerateAvailableNetworkGroups | Enumerates wireless LAN information on available networks and group of associated BSS networks.    |
| EnumerateBssNetworks            | Enumerates wireless LAN information on BSS networks.                                               |
| EnumerateProfileNames           | Enumerates wireless profile names in preference order.                                             |
| EnumerateProfiles               | Enumerates wireless profile information in preference order.                                       |
| EnumerateProfileRadios          | Enumerates wireless profile and related radio information in preference order.                     |
| SetProfile                      | Sets (add or overwrite) the content of a specified wireless profile.                               |
| SetProfilePosition              | Sets the position of a specified wireless profile in preference order.                             |
| SetProfileEapXmlUserData        | Sets (add or overwirte) the EAP user credentials for a specified wireless profile.                 |
| RenameProfile                   | Renames a specified wireless profile.                                                              |
| DeleteProfile                   | Deletes a specified wireless profile.                                                              |
| ConnectNetwork                  | Attempts to connect to the wireless LAN associated to a specified wireless profile.                |
| ConnectNetworkAsync             | Asynchronously attempts to connect to the wireless LAN associated to a specified wireless profile. |
| DisconnectNetwork               | Disconnects from the wireless LAN associated to a specified wireless interface.                    |
| DisconnectNetworkAsync          | Asynchronously disconnects from the wireless LAN associated to a specified wireless interface.     |
| GetInterfaceRadio               | Gets wireless interface radio information of a specified wireless interface.                       |
| TurnOnInterfaceRadio            | Turns on the radio of a specified wireless interface (software radio state only).                  |
| TurnOffInterfaceRadio           | Turns off the radio of a specified wireless interface (software radio state only).                 |
| IsInterfaceAutoConfig           | Checks if automatic configuration of a specified wireless interface is enabled.                    |

## Usage

To check SSIDs of currently available wireless LANs, call `EnumerateAvailableNetworkSsids` method.

```csharp
public static IEnumerable<string> EnumerateNetworkSsids()
{
    return NativeWifi.EnumerateAvailableNetworkSsids()
        .Select(x => x.ToString()); // UTF-8 string representation
}
```

In general, a SSID is represented by a UTF-8 string but it is not guaranteed. So if `ToString` method seems not to produce a valid value, try `ToBytes` method instead.

To connect to a wireless LAN, call `ConnectNetworkAsync` asynchronous method.

```csharp
public static async Task<bool> ConnectAsync()
{
    var availableNetwork = NativeWifi.EnumerateAvailableNetworks()
        .Where(x => !string.IsNullOrWhiteSpace(x.ProfileName))
        .OrderByDescending(x => x.SignalQuality)
        .FirstOrDefault();

    if (availableNetwork is null)
        return false;

    return await NativeWifi.ConnectNetworkAsync(
        interfaceId: availableNetwork.Interface.Id,
        profileName: availableNetwork.ProfileName,
        bssType: availableNetwork.BssType,
        timeout: TimeSpan.FromSeconds(10));
}
```

This method returns true if successfully connected to the wireless LAN in contrast to its synchronous sibling, `ConnectNetwork` method, returns true if the request for the connection succeeds and doesn't indicate the result.

To refresh currently available wireless LANs, call `ScanNetworksAsync` method.

```csharp
public static Task RefreshAsync()
{
    return NativeWifi.ScanNetworksAsync(timeout: TimeSpan.FromSeconds(10));
}
```

This method requests wireless interfaces to scan wireless LANs in parallel. It takes no more than 4 seconds.

To delete an existing wireless profile, use `DeleteProfile` method. Please note that a profile name is case-sensitive.

```csharp
public static bool DeleteProfile(string profileName)
{
    var targetProfile = NativeWifi.EnumerateProfiles()
        .Where(x => profileName.Equals(x.Name, StringComparison.Ordinal))
        .FirstOrDefault();

    if (targetProfile is null)
        return false;

    return NativeWifi.DeleteProfile(
        interfaceId: targetProfile.Interface.Id,
        profileName: profileName);
}
```

To check wireless LAN channels that are already used by surrounding access points, call `EnumerateBssNetworks` method and filter the results by signal strength.

```csharp
public static IEnumerable<int> EnumerateNetworkChannels(int signalStrengthThreshold)
{
    return NativeWifi.EnumerateBssNetworks()
        .Where(x => x.SignalStrength > signalStrengthThreshold)
        .Select(x => x.Channel);
}
```

To turn on the radio of a wireless interface, check the current radio state by `GetInterfaceRadio` method and then call `TurnOnInterfaceRadio` method.

```csharp
public static async Task<bool> TurnOnAsync()
{
    var targetInterface = NativeWifi.EnumerateInterfaces()
        .FirstOrDefault(x =>
        {
            var radioSet = NativeWifi.GetInterfaceRadio(x.Id)?.RadioSets.FirstOrDefault();
            if (radioSet is null)
                return false;

            if (!radioSet.HardwareOn.GetValueOrDefault()) // Hardware radio state is off.
                return false;

            return (radioSet.SoftwareOn == false); // Software radio state is off.
        });

    if (targetInterface is null)
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

Please note that this method can only change software radio state and if hardware radio state is off (like hardware Wi-Fi switch is at off position), the radio cannot be turned on programatically.

## History

Ver 2.3 2022-8-1

- __Add:__ PHY types (802.11ad, ax, be)
- __Add:__ PHY type in profile and related radio information

Ver 2.2 2022-7-25

- __Add:__ Method to set the EAP user credentials
- __Add:__ PHY type in BSS network
- __Breaking change:__ .NET Framework 4.6 or older is no longer supported

Ver 2.1 2021-3-30

- __Fix:__ GetInterfaceRadio is enabled to handle invalid capabilities information

Ver 2.0 2021-2-4

- __Add:__ Support for .NET 5.0 and .NET Standard 2.0

Ver 1.8 2020-12-20

 - __Breaking change:__ GetInterfaceAutoConfig is renamed to IsInterfaceAutoConfig

Ver 1.7 2020-9-29

 - __Add:__ WPA3 in authentication method and algorithm

Ver 1.6 2020-8-4

 - __Add:__ Functionality to connect specific access point when connecting network

Ver 1.5 2019-12-1

 - __Add:__ Information obtained by EnumerateAvailableNetworks and EnumerateAvailableNetworkGroups include authentication/cipher algorithms

Ver 1.4 2018-9-2

 - __Add:__ Methods to provide additional information; EnumerateInterfaceConnections, EnumerateAvailableNetworkGroups, EnumerateProfileRadios
 - __Breaking change:__ Radio information related to wireless profiles can be obtained by EnumerateProfileRadios instead of EnumerateProfiles

Ver 1.0 2015-1-30

 - Initial commit

## License

 - MIT License

 [1]: https://docs.microsoft.com/en-us/windows/win32/nativewifi/portal
 [2]: https://www.nuget.org/packages/ManagedNativeWifi
