# Managed Native Wifi

ManagedNativeWifi is a managed implementation of [Native Wifi][1] API. It provides functionality to manage wireless networks, interfaces and profiles.

## Requirements

This library works on Windows and compatible with:

.NET 8.0|.NET Standard 2.0 (including .NET Framework 4.6.1)
-|-

On Windows 11 (24H2) or newer, some methods require user's permission to access location information. Without the permission, UnauthorizedAccessException will be thrown. This permission can be set in Privacy & security > Location settings. 

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
| GetCurrentConnection            | Gets wireless connection information (connected wireless LAN only)                                 |
| GetRssi                         | Gets RSSI (connected wireless LAN only).                                                           |
| GetRealtimeConnetionQuality     | Gets wireless connection quality information (connected wireless LAN only, Windows 11 24H2 only)   |
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
| GetRadio                        | Gets wireless interface radio information of a specified wireless interface.                       |
| TurnOnRadio                     | Turns on the radio of a specified wireless interface (software radio state only).                  |
| TurnOffRadio                    | Turns off the radio of a specified wireless interface (software radio state only).                 |
| IsAutoConfig                    | Checks if automatic configuration of a specified wireless interface is enabled.                    |

## Properties

| Property           | Description                                           |
|--------------------|-------------------------------------------------------|
| ThrowsOnAnyFailure | Whether to throw an exception when any failure occurs |

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

To check SSID, signal quality or other information on currently available wireless LANs, call `EnumerateAvailableNetworks` method.

```csharp
public static IEnumerable<(string ssidString, int signalQuality)>
    EnumerateNetworkSsidsAndSignalQualities()
{
    return NativeWifi.EnumerateAvailableNetworks()
        .Select(x => (x.Ssid.ToString(), x.SignalQuality));
}
```

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
        interfaceId: availableNetwork.InterfaceInfo.Id,
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

This method requests wireless interfaces to scan wireless LANs in parallel. The timeout should be ideally no more than 4 seconds, but it can vary depending on the situation.

If you want to avoid disrupting existing wireless connections, you can use `ScanNetworksAsync` overload method with `ScanMode.OnlyNotConnected`.

```csharp
public static Task RefreshNotConnectedAsync()
{
    return NativeWifi.ScanNetworksAsync(
        mode: ScanMode.OnlyNotConnected,
        null,
        null,
        timeout: TimeSpan.FromSeconds(10),
        CancellationToken.None);
}
```

Please note that if all wireless interfaces are connected, naturally nothing will happen.

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
        interfaceId: targetProfile.InterfaceInfo.Id,
        profileName: profileName);
}
```

To check wireless LAN channels that are already used by surrounding access points, call `EnumerateBssNetworks` method and filter the results by RSSI.

```csharp
public static IEnumerable<int> EnumerateNetworkChannels(int rssiThreshold)
{
    return NativeWifi.EnumerateBssNetworks()
        .Where(x => x.Rssi > rssiThreshold)
        .Select(x => x.Channel);
}
```

To turn on the radio of a wireless interface, check the current radio state by `GetRadio` method and then call `TurnOnRadio` method.

```csharp
public static async Task<bool> TurnOnAsync()
{
    var targetInterface = NativeWifi.EnumerateInterfaces()
        .FirstOrDefault(x =>
        {
            var radioState = NativeWifi.GetRadio(x.Id)?.RadioStates.FirstOrDefault();
            if (radioState is null)
                return false;

            if (!radioState.IsHardwareOn) // Hardware radio state is off.
                return false;

            return !radioState.IsSoftwareOn; // Software radio state is off.
        });

    if (targetInterface is null)
        return false;

    try
    {
        return await Task.Run(() => NativeWifi.TurnOnRadio(targetInterface.Id));
    }
    catch (UnauthorizedAccessException)
    {
        return false;
    }
}
```

Please note that this method can only change software radio state and if hardware radio state is off (like hardware Wi-Fi switch is at off position), the radio cannot be turned on programatically.

To retrieve detailed information on wireless connections of connected wireless interfaces, you can use `GetCurrentConnection`, `GetRssi`, `GetRealtimeConnectionQuality` methods depending on your needs.

```csharp
public static void ShowConnectedNetworkInformation()
{
    foreach (var interfaceId in NativeWifi.EnumerateInterfaces()
        .Where(x => x.State is InterfaceState.Connected)
        .Select(x => x.Id))
    {
        // Following methods work only with connected wireless interfaces.
        var (result, cc) = NativeWifi.GetCurrentConnection(interfaceId);
        if (result is ActionResult.Success)
        {
            Trace.WriteLine($"Profile: {cc.ProfileName}");
            Trace.WriteLine($"SSID: {cc.Ssid}");
            Trace.WriteLine($"PHY type: 802.11{cc.PhyType.ToProtocolName()}");
            Trace.WriteLine($"Authentication algorithm: {cc.AuthenticationAlgorithm}");
            Trace.WriteLine($"Cipher algorithm: {cc.CipherAlgorithm}");
            Trace.WriteLine($"Signal quality: {cc.SignalQuality}");
            Trace.WriteLine($"Rx rate: {cc.RxRate} Kbps");
            Trace.WriteLine($"Tx rate: {cc.TxRate} Kbps");
        }

        // GetRealtimeConnectionQuality method works only on Windows 11 24H2.
        (result, var rcq) = NativeWifi.GetRealtimeConnectionQuality(interfaceId);
        if (result is ActionResult.Success)
        {
            Trace.WriteLine($"PHY type: 802.11{rcq.PhyType.ToProtocolName()}");
            Trace.WriteLine($"Link quality: {rcq.LinkQuality}");
            Trace.WriteLine($"Rx rate: {rcq.RxRate} Kbps");
            Trace.WriteLine($"Tx rate: {rcq.TxRate} Kbps");
            Trace.WriteLine($"MLO connection: {rcq.IsMultiLinkOperation}");

            if (rcq.Links.Count > 0)
            {
                var link = rcq.Links[0];
                Trace.WriteLine($"RSSI: {link.Rssi}");
                Trace.WriteLine($"Frequency: {link.Frequency} MHz");
                Trace.WriteLine($"Bandwidth: {link.Bandwidth} MHz");
            }
        }
        else if (result is ActionResult.NotSupported)
        {
            (result, int rssi) = NativeWifi.GetRssi(interfaceId);
            if (result is ActionResult.Success)
            {
                Trace.WriteLine($"RSSI: {rssi}");
            }
        }
    }
}
```

The returned Tuple has `result` member and if the wireless interface is not connected, it will be `ActionResult.NotConnected`.

## Remarks

- Creating a wireless profile from scratch is not covered in this library. It is because 1) Native WiFi does not include such functionality, 2) it requires careful consideration on wi-fi technology in use, 3) it involves sensitive security information. Thus, it is left to each user.

## Release Notes

Ver 3.0.2 2025-7-5

 - __Fix:__ RadioStateChanged & SignalQualityChanged events are not prevented by ScanNetworksAsync, ConnectNetworkAsync or DisconnectNetworkAsync methods.

Ver 3.0.1 2025-7-4

 - __Add:__ 
   * ScanNetworksAsync overload method for specified SSID
   * EnumerateAvailableNetworks overload method for specified wireless interface
   * EnumerateBssNetworks overload method for specified wireless interface
   * GetCurrentConnection method for specified wireless interface - This works only with connected wireless LAN.
   * GetRssi method for specified wireless interface - This works only with connected wireless LAN.
   * GetRealtimeConnetionQuality method for specified wireless interface - This works only with connected wireless LAN and only on Windows 11 24H2.
   * IsConnectable property to AvailableNetworkPack - This affects EnumerateAvailableNetworks & EnumerateAvailableNetworkGroups methods.
   * New values of CipherAlgorithm, EncryptionType, BssType enum
 - __Breaking change:__
   * Remove: ConnectionMode & ProfileName properties from InterfaceConnectionInfo - This affects EnumerateInterfaceConnections method. If those properties are necessary, use GetCurrentConnection method.
   * Rename: OnlyDisconnectd of ScanMode enum to OnlyNotConnected
   * Rename: SignalStrength property of BssNetworkPack to Rssi - This affects EnumerateBssNetworks method.
   * Rename: GetInterfaceRadio method to GetRadio
   * Rename: TurnOnInterfaceRadio method to TurnOnRadio
   * Rename: TurnOffInterfaceRadio method to TurnOffRadio
   * Rename: IsInterfaceAutoConfig method to IsAutoConfig
   * Remove: Id property of RadioInfo - This affects GetRadio method.
   * Replace: RadioSet & PhyRadioStateInfo with RadioStateSet - This affects GetRadio method & RadioStateChangedEventArgs event args.
   * Change: Type of bssid parameter of ConnectNetwork & ConnectNetworkAsync methods

Ver 2.8 2025-5-18

 - __Add:__ ScanNetworksAsync overload methods with modes for only disconnected interfaces or only specified interfaces
 - __Add:__ RadioStateChanged, SignalQualityChanged events to instantiatable implementation

Ver 2.7.1 2025-4-10

 - __Fix:__ WPA3 Enterprise is correctly assigned.

Ver 2.7 2025-1-25

 - __Add & Breaking change:__ WPA3 authentications are re-designated, and WPA3 is removed and superseded by WPA3 Enterprise 192-bit mode as WPA3 is deprecated.

  | Authentication               | AuthenticationAlgorithm | AuthenticationMethod |
  |------------------------------|-------------------------|----------------------|
  | WPA3 Enterprise 192-bit mode | WPA3_ENT_192            | WPA3_Enterprise_192  |
  | WPA3 Enterprise              | WPA3_ENT                | WPA3_Enterprise      |
  | OWE                          | OWE                     | OWE                  |

 - __Breaking change:__ .NET 6.0, 7.0 are no longer supported

Ver 2.6 2024-7-5

 - __Add:__ 6GHz channels

Ver 2.5 2023-1-9

 - __Add:__ ThrowsOnAnyFailure property to throw an exception when any failure occurs

Ver 2.4 2022-11-24

 - __Add:__ Special event args for AvailabilityChanged, InterfaceChanged, ConnectionChanged, ProfileChanged events
 - __Breaking change:__ .NET 5.0 is no longer supported.

Ver 2.3 2022-8-1

 - __Add:__ PHY types (802.11ad, ax, be)
 - __Add:__ PHY type in profile and related radio information

Ver 2.2 2022-7-25

 - __Add:__ SetProfileEapXmlUserData method to set the EAP user credentials
 - __Add:__ PHY type to BSS network
 - __Breaking change:__ .NET Framework 4.6 or older is no longer supported

Ver 2.1 2021-3-30

 - __Fix:__ GetInterfaceRadio is enabled to handle invalid capabilities information.

Ver 2.0 2021-2-4

 - __Add:__ Support for .NET 5.0 and .NET Standard 2.0

Ver 1.8 2020-12-20

 - __Breaking change:__ GetInterfaceAutoConfig method is renamed to IsInterfaceAutoConfig.

Ver 1.7 2020-9-29

 - __Add:__ WPA3 in authentication method and algorithm

Ver 1.6 2020-8-4

 - __Add:__ Functionality to connect specific access point when connecting wireless LAN

Ver 1.5 2019-12-1

 - __Add:__ Authentication/Cipher algorithms to information obtained by EnumerateAvailableNetworks & EnumerateAvailableNetworkGroups methods

Ver 1.4 2018-9-2

 - __Add:__ EnumerateInterfaceConnections, EnumerateAvailableNetworkGroups, EnumerateProfileRadios methods
 - __Breaking change:__ Radio information related to wireless profiles can be obtained by EnumerateProfileRadios instead of EnumerateProfiles.

Ver 1.0 2015-1-30

 - Initial commit

## License

 - MIT License

 [1]: https://docs.microsoft.com/en-us/windows/win32/nativewifi/portal
 [2]: https://www.nuget.org/packages/ManagedNativeWifi
