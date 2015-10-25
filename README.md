Managed Native Wifi
===================

A managed implementation of [Native Wifi][1] API

##Requirements

 * .NET Framework 4.5.2

##Methods

Available methods including asynchronous ones based on TAP.

| Methods                        | Description                                                                                       |
|--------------------------------|---------------------------------------------------------------------------------------------------|
| ScanAsync                      | Asynchronously request wireless interfaces to scan (rescan) wireless LANs.                        |
| EnumerateAvailableNetworkSsids | Enumerate SSIDs of available wireless LANs.                                                       |
| EnumerateConnectedNetworkSsids | Enumerate SSIDs of connected wireless LANs.                                                       |
| EnumerateAvailableNetworks     | Enumerate wireless LAN information on available networks.                                         |
| EnumerateBssNetworks           | Enumerate wireless LAN information on BSS networks.                                               |
| EnumerateProfileNames          | Enumerate wireless profile names in preference order.                                             |
| EnumerateProfiles              | Enumerate wireless profile information in preference order.                                       |
| SetProfilePosition             | Set the position of a specified wireless profile in preference order.                             |
| DeleteProfile                  | Delete a specified wireless profile.                                                              |
| Connect                        | Attempt to connect to the wireless LAN associated to a specified wireless profile.                |
| ConnectAsync                   | Asynchronously attempt to connect to the wireless LAN associated to a specified wireless profile. |
| Disconnect                     | Disconnect from the wireless LAN associated to a specified wireless interface.                    |
| DisconnectAsync                | Asynchronously disconnect from the wireless LAN associated to a specified wireless interface.     |

##License

 - MIT License

[1]: https://msdn.microsoft.com/en-us/library/windows/desktop/ms706556.aspx
