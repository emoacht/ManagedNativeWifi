
namespace ManagedNativeWifi;

/// <summary>
/// Reason code for wireless LAN operations
/// Each underlying value is the same as equivalent WLAN_REASON_CODE.
/// </summary>
/// <remarks>
/// Most meanings are retrieved from WlanReasonCodeToString function and supplemented by
/// the meaning column of WLAN_REASON_CODE enumeration:
/// https://learn.microsoft.com/en-us/windows/win32/nativewifi/wlan-reason-code
/// </remarks>
public enum ReasonCode : uint
{
	/// <summary>
	/// WLAN_REASON_CODE_SUCCESS
	/// Successful.
	/// </summary>
	Success = 0x00000000,
	/// <summary>
	/// WLAN_REASON_CODE_UNKNOWN
	/// Failed for unknown reason.
	/// </summary>
	Unknown = 0x00010001,
	/// <summary>
	/// WLAN_REASON_CODE_RANGE_SIZE
	/// </summary>
	RangeSize = 0x00010000,
	/// <summary>
	/// WLAN_REASON_CODE_BASE
	/// </summary>
	Base = 0x00020000,
	/// <summary>
	/// WLAN_REASON_CODE_AC_BASE
	/// </summary>
	AcBase = 0x00020000,
	/// <summary>
	/// WLAN_REASON_CODE_AC_CONNECT_BASE
	/// </summary>
	AcConnectBase = 0x00028000,
	/// <summary>
	/// WLAN_REASON_CODE_AC_END
	/// </summary>
	AcEnd = 0x0002FFFF,
	/// <summary>
	/// WLAN_REASON_CODE_PROFILE_BASE
	/// </summary>
	ProfileBase = 0x00080000,
	/// <summary>
	/// WLAN_REASON_CODE_PROFILE_CONNECT_BASE
	/// </summary>
	ProfileConnectBase = 0x00088000,
	/// <summary>
	/// WLAN_REASON_CODE_PROFILE_END
	/// </summary>
	ProfileEnd = 0x0008FFFF,
	/// <summary>
	/// WLAN_REASON_CODE_MSM_BASE
	/// </summary>
	MsmBase = 0x00030000,
	/// <summary>
	/// WLAN_REASON_CODE_MSM_CONNECT_BASE
	/// </summary>
	MsmConnectBase = 0x00038000,
	/// <summary>
	/// WLAN_REASON_CODE_MSM_END
	/// </summary>
	MsmEnd = 0x0003FFFF,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_BASE
	/// </summary>
	MsmsecBase = 0x00040000,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CONNECT_BASE
	/// </summary>
	MsmsecConnectBase = 0x00048000,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_END
	/// </summary>
	MsmsecEnd = 0x0004FFFF,
	/// <summary>
	/// WLAN_REASON_CODE_RESERVED_BASE
	/// </summary>
	ReservedBase = 0x000B0000,
	/// <summary>
	/// WLAN_REASON_CODE_RESERVED_END
	/// </summary>
	ReservedEnd = 0x000BFFFF,
	/// <summary>
	/// WLAN_REASON_CODE_NETWORK_NOT_COMPATIBLE
	/// The wireless network is not compatible.
	/// </summary>
	NetworkNotCompatible = 0x00020001,
	/// <summary>
	/// WLAN_REASON_CODE_PROFILE_NOT_COMPATIBLE
	/// The profile for the wireless network is not compatible.
	/// </summary>
	ProfileNotCompatible = 0x00020002,
	/// <summary>
	/// WLAN_REASON_CODE_NO_AUTO_CONNECTION
	/// The profile is not set to connect automatically.
	/// </summary>
	NoAutoConnection = 0x00028001,
	/// <summary>
	/// WLAN_REASON_CODE_NOT_VISIBLE
	/// The wireless network is not visible.
	/// </summary>
	NotVisible = 0x00028002,
	/// <summary>
	/// WLAN_REASON_CODE_GP_DENIED
	/// The wireless network is blocked by group policy.
	/// </summary>
	GpDenied = 0x00028003,
	/// <summary>
	/// WLAN_REASON_CODE_USER_DENIED
	/// The wireless network is blocked by user.
	/// </summary>
	UserDenied = 0x00028004,
	/// <summary>
	/// WLAN_REASON_CODE_BSS_TYPE_NOT_ALLOWED
	/// The BSS type is not allowed on this wireless adapter.
	/// </summary>
	BssTypeNotAllowed = 0x00028005,
	/// <summary>
	/// WLAN_REASON_CODE_IN_FAILED_LIST
	/// The wireless network is in the failed list.
	/// </summary>
	InFailedList = 0x00028006,
	/// <summary>
	/// WLAN_REASON_CODE_IN_BLOCKED_LIST
	/// The wireless network is in the blocked list.
	/// </summary>
	InBlockedList = 0x00028007,
	/// <summary>
	/// WLAN_REASON_CODE_SSID_LIST_TOO_LONG
	/// The size of the SSID list exceeds the maximum size supported by the adapter.
	/// </summary>
	SsidListTooLong = 0x00028008,
	/// <summary>
	/// WLAN_REASON_CODE_CONNECT_CALL_FAIL
	/// The MSM connect call failed.
	/// </summary>
	ConnectCallFail = 0x00028009,
	/// <summary>
	/// WLAN_REASON_CODE_SCAN_CALL_FAIL
	/// The MSM scan call failed.
	/// </summary>
	ScanCallFail = 0x0002800A,
	/// <summary>
	/// WLAN_REASON_CODE_NETWORK_NOT_AVAILABLE
	/// The specified network is not available.
	/// </summary>
	NetworkNotAvailable = 0x0002800B,
	/// <summary>
	/// WLAN_REASON_CODE_PROFILE_CHANGED_OR_DELETED
	/// The profile used for the connection has changed or been deleted.
	/// </summary>
	ProfileChangedOrDeleted = 0x0002800C,
	/// <summary>
	/// WLAN_REASON_CODE_KEY_MISMATCH
	/// The password for the network might not be correct.
	/// </summary>
	KeyMismatch = 0x0002800D,
	/// <summary>
	/// WLAN_REASON_CODE_USER_NOT_RESPOND
	/// The information required from the user was not received.
	/// </summary>
	UserNotRespond = 0x0002800E,
	/// <summary>
	/// WLAN_REASON_CODE_AP_PROFILE_NOT_ALLOWED_FOR_CLIENT
	/// Access Point profile is not allowed for old version client.
	/// </summary>
	ApProfileNotAllowedForClient = 0x0002800F,
	/// <summary>
	/// WLAN_REASON_CODE_AP_PROFILE_NOT_ALLOWED
	/// Access Point profile is not allowed by user or group policy.
	/// </summary>
	ApProfileNotAllowed = 0x00028010,
	/// <summary>
	/// WLAN_REASON_CODE_HOTSPOT2_PROFILE_DENIED
	/// </summary>
	Hotspot2ProfileDenied = 0x00028011,
	/// <summary>
	/// WLAN_REASON_CODE_INVALID_PROFILE_SCHEMA
	/// The profile is invalid according to the schema.
	/// </summary>
	InvalidProfileSchema = 0x00080001,
	/// <summary>
	/// WLAN_REASON_CODE_PROFILE_MISSING
	/// The profile element is missing.
	/// </summary>
	ProfileMissing = 0x00080002,
	/// <summary>
	/// WLAN_REASON_CODE_INVALID_PROFILE_NAME
	/// The name of the profile is invalid.
	/// </summary>
	InvalidProfileName = 0x00080003,
	/// <summary>
	/// WLAN_REASON_CODE_INVALID_PROFILE_TYPE
	/// The type of the profile is invalid.
	/// </summary>
	InvalidProfileType = 0x00080004,
	/// <summary>
	/// WLAN_REASON_CODE_INVALID_PHY_TYPE
	/// The PHY type is invalid.
	/// </summary>
	InvalidPhyType = 0x00080005,
	/// <summary>
	/// WLAN_REASON_CODE_MSM_SECURITY_MISSING
	/// The MSM security settings are missing.
	/// </summary>
	MsmSecurityMissing = 0x00080006,
	/// <summary>
	/// WLAN_REASON_CODE_IHV_SECURITY_NOT_SUPPORTED
	/// The IHV security settings are not supported.
	/// </summary>
	IhvSecurityNotSupported = 0x00080007,
	/// <summary>
	/// WLAN_REASON_CODE_IHV_OUI_MISMATCH
	/// The IHV profile OUI did not match with the adapter OUI.
	/// </summary>
	IhvOuiMismatch = 0x00080008,
	/// <summary>
	/// WLAN_REASON_CODE_IHV_OUI_MISSING
	/// The IHV OUI settings are missing.
	/// </summary>
	IhvOuiMissing = 0x00080009,
	/// <summary>
	/// WLAN_REASON_CODE_IHV_SETTINGS_MISSING
	/// The IHV security settings are missing.
	/// </summary>
	IhvSettingsMissing = 0x0008000A,
	/// <summary>
	/// WLAN_REASON_CODE_CONFLICT_SECURITY
	/// Both Microsoft and IHV security settings exist in the profile.
	/// </summary>
	ConflictSecurity = 0x0008000B,
	/// <summary>
	/// WLAN_REASON_CODE_SECURITY_MISSING
	/// No security settings exist in the profile.
	/// </summary>
	SecurityMissing = 0x0008000C,
	/// <summary>
	/// WLAN_REASON_CODE_INVALID_BSS_TYPE
	/// The BSS type is not valid.
	/// </summary>
	InvalidBssType = 0x0008000D,
	/// <summary>
	/// WLAN_REASON_CODE_INVALID_ADHOC_CONNECTION_MODE
	/// Automatic connection cannot be set for an ad hoc network.
	/// </summary>
	InvalidAdhocConnectionMode = 0x0008000E,
	/// <summary>
	/// WLAN_REASON_CODE_NON_BROADCAST_SET_FOR_ADHOC
	/// Non-broadcast cannot be set for an ad hoc network.
	/// </summary>
	NonBroadcastSetForAdhoc = 0x0008000F,
	/// <summary>
	/// WLAN_REASON_CODE_AUTO_SWITCH_SET_FOR_ADHOC
	/// Auto-switch cannot be set for an ad hoc network.
	/// </summary>
	AutoSwitchSetForAdhoc = 0x00080010,
	/// <summary>
	/// WLAN_REASON_CODE_AUTO_SWITCH_SET_FOR_MANUAL_CONNECTION
	/// Auto-switch cannot be set for a manual connection profile.
	/// </summary>
	AutoSwitchSetForManualConnection = 0x00080011,
	/// <summary>
	/// WLAN_REASON_CODE_IHV_SECURITY_ONEX_MISSING
	/// The IHV 802.1X security settings are missing.
	/// </summary>
	IhvSecurityOnexMissing = 0x00080012,
	/// <summary>
	/// WLAN_REASON_CODE_PROFILE_SSID_INVALID
	/// The SSID in the profile is invalid or missing.
	/// </summary>
	ProfileSsidInvalid = 0x00080013,
	/// <summary>
	/// WLAN_REASON_CODE_TOO_MANY_SSID
	/// Too many SSIDs specified in a profile.
	/// </summary>
	TooManySsid = 0x00080014,
	/// <summary>
	/// WLAN_REASON_CODE_IHV_CONNECTIVITY_NOT_SUPPORTED
	/// The IHV connectivity settings are not supported.
	/// </summary>
	IhvConnectivityNotSupported = 0x00080015,
	/// <summary>
	/// WLAN_REASON_CODE_BAD_MAX_NUMBER_OF_CLIENTS_FOR_AP
	/// The maximum number of clients is not a valid value in Access Point mode.
	/// </summary>
	BadMaxNumberOfClientsForAp = 0x00080016,
	/// <summary>
	/// WLAN_REASON_CODE_INVALID_CHANNEL
	/// The specified channel is not valid.
	/// </summary>
	InvalidChannel = 0x00080017,
	/// <summary>
	/// WLAN_REASON_CODE_OPERATION_MODE_NOT_SUPPORTED
	/// The operation mode is not supported by the driver.
	/// </summary>
	OperationModeNotSupported = 0x00080018,
	/// <summary>
	/// WLAN_REASON_CODE_AUTO_AP_PROFILE_NOT_ALLOWED
	/// Auto-connect or auto-switch should not be set for an Access Point profile.
	/// </summary>
	AutoApProfileNotAllowed = 0x00080019,
	/// <summary>
	/// WLAN_REASON_CODE_AUTO_CONNECTION_NOT_ALLOWED
	/// Automatic connection is not allowed.
	/// </summary>
	AutoConnectionNotAllowed = 0x0008001A,
	/// <summary>
	/// WLAN_REASON_CODE_HOTSPOT2_PROFILE_NOT_ALLOWED
	/// </summary>
	Hotspot2ProfileNotAllowed = 0x0008001B,
	/// <summary>
	/// WLAN_REASON_CODE_UNSUPPORTED_SECURITY_SET_BY_OS
	/// The security settings are not supported by the operating system.
	/// </summary>
	UnsupportedSecuritySetByOs = 0x00030001,
	/// <summary>
	/// WLAN_REASON_CODE_UNSUPPORTED_SECURITY_SET
	/// The security settings are not supported.
	/// </summary>
	UnsupportedSecuritySet = 0x00030002,
	/// <summary>
	/// WLAN_REASON_CODE_BSS_TYPE_UNMATCH
	/// The BSS type does not match.
	/// </summary>
	BssTypeUnmatch = 0x00030003,
	/// <summary>
	/// WLAN_REASON_CODE_PHY_TYPE_UNMATCH
	/// The PHY type does not match.
	/// </summary>
	PhyTypeUnmatch = 0x00030004,
	/// <summary>
	/// WLAN_REASON_CODE_DATARATE_UNMATCH
	/// The data rate does not match.
	/// </summary>
	DatarateUnmatch = 0x00030005,
	/// <summary>
	/// WLAN_REASON_CODE_USER_CANCELLED
	/// The operation was cancelled by user.
	/// </summary>
	UserCancelled = 0x00038001,
	/// <summary>
	/// WLAN_REASON_CODE_ASSOCIATION_FAILURE
	/// The driver disconnected while associating.
	/// </summary>
	AssociationFailure = 0x00038002,
	/// <summary>
	/// WLAN_REASON_CODE_ASSOCIATION_TIMEOUT
	/// Association with the network timed out.
	/// </summary>
	AssociationTimeout = 0x00038003,
	/// <summary>
	/// WLAN_REASON_CODE_PRE_SECURITY_FAILURE
	/// Pre-association security failed.
	/// </summary>
	PreSecurityFailure = 0x00038004,
	/// <summary>
	/// WLAN_REASON_CODE_START_SECURITY_FAILURE
	/// Failed to start security after association.
	/// </summary>
	StartSecurityFailure = 0x00038005,
	/// <summary>
	/// WLAN_REASON_CODE_SECURITY_FAILURE
	/// Security failed.
	/// </summary>
	SecurityFailure = 0x00038006,
	/// <summary>
	/// WLAN_REASON_CODE_SECURITY_TIMEOUT
	/// Security operation times out.
	/// </summary>
	SecurityTimeout = 0x00038007,
	/// <summary>
	/// WLAN_REASON_CODE_ROAMING_FAILURE
	/// The driver disconnected while roaming.
	/// </summary>
	RoamingFailure = 0x00038008,
	/// <summary>
	/// WLAN_REASON_CODE_ROAMING_SECURITY_FAILURE
	/// Failed to start security for roaming.
	/// </summary>
	RoamingSecurityFailure = 0x00038009,
	/// <summary>
	/// WLAN_REASON_CODE_ADHOC_SECURITY_FAILURE
	/// Failed to start security for Ad hoc peer.
	/// </summary>
	AdhocSecurityFailure = 0x0003800A,
	/// <summary>
	/// WLAN_REASON_CODE_DRIVER_DISCONNECTED
	/// The driver disconnected.
	/// </summary>
	DriverDisconnected = 0x0003800B,
	/// <summary>
	/// WLAN_REASON_CODE_DRIVER_OPERATION_FAILURE
	/// The driver failed to perform some operations.
	/// </summary>
	DriverOperationFailure = 0x0003800C,
	/// <summary>
	/// WLAN_REASON_CODE_IHV_NOT_AVAILABLE
	/// The IHV service is not available.
	/// </summary>
	IhvNotAvailable = 0x0003800D,
	/// <summary>
	/// WLAN_REASON_CODE_IHV_NOT_RESPONDING
	/// The IHV service timed out.
	/// </summary>
	IhvNotResponding = 0x0003800E,
	/// <summary>
	/// WLAN_REASON_CODE_DISCONNECT_TIMEOUT
	/// The driver disconnect timed out.
	/// </summary>
	DisconnectTimeout = 0x0003800F,
	/// <summary>
	/// WLAN_REASON_CODE_INTERNAL_FAILURE
	/// An internal failure prevented the operation from completing.
	/// </summary>
	InternalFailure = 0x00038010,
	/// <summary>
	/// WLAN_REASON_CODE_UI_REQUEST_TIMEOUT
	/// The user interaction request timed out.
	/// </summary>
	UiRequestTimeout = 0x00038011,
	/// <summary>
	/// WLAN_REASON_CODE_TOO_MANY_SECURITY_ATTEMPTS
	/// The computer is roaming too often. The security check did not complete after several attempts.
	/// </summary>
	TooManySecurityAttempts = 0x00038012,
	/// <summary>
	/// WLAN_REASON_CODE_AP_STARTING_FAILURE
	/// The driver failed to start as Access Point.
	/// </summary>
	ApStartingFailure = 0x00038013,
	/// <summary>
	/// WLAN_REASON_CODE_NO_VISIBLE_AP
	/// Failed to connect because no connectable Access Point was visible.
	/// </summary>
	NoVisibleAp = 0x00038014,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_MIN
	/// </summary>
	MsmsecMin = 0x00040000,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_INVALID_KEY_INDEX
	/// Key index specified is not valid.
	/// </summary>
	MsmsecProfileInvalidKeyIndex = 0x00040001,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_PSK_PRESENT
	/// Key required, PSK present.
	/// </summary>
	MsmsecProfilePskPresent = 0x00040002,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_KEY_LENGTH
	/// Invalid key length.
	/// </summary>
	MsmsecProfileKeyLength = 0x00040003,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_PSK_LENGTH
	/// Invalid PSK length.
	/// </summary>
	MsmsecProfilePskLength = 0x00040004,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_NO_AUTH_CIPHER_SPECIFIED
	/// No auth/cipher are specified.
	/// </summary>
	MsmsecProfileNoAuthCipherSpecified = 0x00040005,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_TOO_MANY_AUTH_CIPHER_SPECIFIED
	/// Too many auth/cipher are specified.
	/// </summary>
	MsmsecProfileTooManyAuthCipherSpecified = 0x00040006,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_DUPLICATE_AUTH_CIPHER
	/// The profile contains duplicate auth/cipher.
	/// </summary>
	MsmsecProfileDuplicateAuthCipher = 0x00040007,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_RAWDATA_INVALID
	/// The profile raw data is invalid.
	/// </summary>
	MsmsecProfileRawdataInvalid = 0x00040008,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_INVALID_AUTH_CIPHER
	/// Invalid auth/cipher combination.
	/// </summary>
	MsmsecProfileInvalidAuthCipher = 0x00040009,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_ONEX_DISABLED
	/// 802.1x disabled when it is required to be enabled.
	/// </summary>
	MsmsecProfileOnexDisabled = 0x0004000A,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_ONEX_ENABLED
	/// 802.1x enabled when it is required to be disabled.
	/// </summary>
	MsmsecProfileOnexEnabled = 0x0004000B,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_INVALID_PMKCACHE_MODE
	/// Invalid PMK cache mode.
	/// </summary>
	MsmsecProfileInvalidPmkcacheMode = 0x0004000C,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_INVALID_PMKCACHE_SIZE
	/// Invalid PMK cache size.
	/// </summary>
	MsmsecProfileInvalidPmkcacheSize = 0x0004000D,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_INVALID_PMKCACHE_TTL
	/// Invalid PMK cache TTL.
	/// </summary>
	MsmsecProfileInvalidPmkcacheTtl = 0x0004000E,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_INVALID_PREAUTH_MODE
	/// Invalid PreAuth mode.
	/// </summary>
	MsmsecProfileInvalidPreauthMode = 0x0004000F,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_INVALID_PREAUTH_THROTTLE
	/// Invalid PreAuth throttle.
	/// </summary>
	MsmsecProfileInvalidPreauthThrottle = 0x00040010,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_PREAUTH_ONLY_ENABLED
	/// PreAuth enabled when PMK cache is disabled.
	/// </summary>
	MsmsecProfilePreauthOnlyEnabled = 0x00040011,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CAPABILITY_NETWORK
	/// Capability matching failed at network.
	/// </summary>
	MsmsecCapabilityNetwork = 0x00040012,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CAPABILITY_NIC
	/// Capability matching failed at NIC.
	/// </summary>
	MsmsecCapabilityNic = 0x00040013,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CAPABILITY_PROFILE
	/// Capability matching failed at profile.
	/// </summary>
	MsmsecCapabilityProfile = 0x00040014,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CAPABILITY_DISCOVERY
	/// The network does not support specified discovery type.
	/// </summary>
	MsmsecCapabilityDiscovery = 0x00040015,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_PASSPHRASE_CHAR
	/// Passphrase contains invalid character.
	/// </summary>
	MsmsecProfilePassphraseChar = 0x00040016,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_KEYMATERIAL_CHAR
	/// Key material contains invalid character.
	/// </summary>
	MsmsecProfileKeymaterialChar = 0x00040017,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_WRONG_KEYTYPE
	/// The key type specified does not match the key material.
	/// </summary>
	MsmsecProfileWrongKeytype = 0x00040018,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_MIXED_CELL
	/// Mixed cell is suspected. The Access Point is not signaling that it is compatible with a privacy-enabled profile.
	/// </summary>
	MsmsecMixedCell = 0x00040019,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_AUTH_TIMERS_INVALID
	/// 802.1x authentication timers in the profile are invalid.
	/// </summary>
	MsmsecProfileAuthTimersInvalid = 0x0004001A,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_INVALID_GKEY_INTV
	/// Group key update interval in profile is invalid.
	/// </summary>
	MsmsecProfileInvalidGkeyIntv = 0x0004001B,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_TRANSITION_NETWORK
	/// Transition network is suspected. Legacy 802.11 security is used for the next authentication attempt.
	/// </summary>
	MsmsecTransitionNetwork = 0x0004001C,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_KEY_UNMAPPED_CHAR
	/// The key contains characters that are not in the ASCII character set.
	/// </summary>
	MsmsecProfileKeyUnmappedChar = 0x0004001D,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CAPABILITY_PROFILE_AUTH
	/// Capability matching failed because the network does not support the authentication method in the profile.
	/// </summary>
	MsmsecCapabilityProfileAuth = 0x0004001E,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CAPABILITY_PROFILE_CIPHER
	/// Capability matching failed at profile because the network does not support the cipher algorithm in the profile.
	/// </summary>
	MsmsecCapabilityProfileCipher = 0x0004001F,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_SAFE_MODE
	/// The FIPS 140-2 mode value is not valid.
	/// </summary>
	MsmsecProfileSafeMode = 0x00040020,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CAPABILITY_PROFILE_SAFE_MODE_NIC
	/// The profile requires FIPS 140-2 mode, which is not supported by the network adapter.
	/// </summary>
	MsmsecCapabilityProfileSafeModeNic = 0x00040021,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CAPABILITY_PROFILE_SAFE_MODE_NW
	/// The profile requires FIPS 140-2 mode, which is not supported by the network.
	/// </summary>
	MsmsecCapabilityProfileSafeModeNw = 0x00040022,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_UNSUPPORTED_AUTH
	/// The authentication algorithm in the profile is not supported.
	/// </summary>
	MsmsecProfileUnsupportedAuth = 0x00040023,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PROFILE_UNSUPPORTED_CIPHER
	/// The cipher algorithm in the profile is not supported.
	/// </summary>
	MsmsecProfileUnsupportedCipher = 0x00040024,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CAPABILITY_MFP_NW_NIC
	/// The wireless network requires Management Frame Protection (MFP) and the network interface does not support MFP.
	/// </summary>
	MsmsecCapabilityMfpNwNic = 0x00040025,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_UI_REQUEST_FAILURE
	/// Failed to queue the user interface request.
	/// </summary>
	MsmsecUiRequestFailure = 0x00048001,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_AUTH_START_TIMEOUT
	/// 802.1x authentication did not start within configured time.
	/// </summary>
	MsmsecAuthStartTimeout = 0x00048002,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_AUTH_SUCCESS_TIMEOUT
	/// 802.1x authentication did not complete within configured time.
	/// </summary>
	MsmsecAuthSuccessTimeout = 0x00048003,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_KEY_START_TIMEOUT
	/// Dynamic key exchange did not start within configured time.
	/// </summary>
	MsmsecKeyStartTimeout = 0x00048004,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_KEY_SUCCESS_TIMEOUT
	/// Dynamic key exchange did not succeed within configured time.
	/// </summary>
	MsmsecKeySuccessTimeout = 0x00048005,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_M3_MISSING_KEY_DATA
	/// Message 3 of 4 way handshake has no key data (RSN/WPA).
	/// </summary>
	MsmsecM3MissingKeyData = 0x00048006,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_M3_MISSING_IE
	/// Message 3 of 4 way handshake has no IE (RSN/WPA).
	/// </summary>
	MsmsecM3MissingIe = 0x00048007,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_M3_MISSING_GRP_KEY
	/// Message 3 of 4 way handshake has no group key (RSN).
	/// </summary>
	MsmsecM3MissingGrpKey = 0x00048008,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PR_IE_MATCHING
	/// Matching security capabilities of IE in M3 failed (RSN/WPA).
	/// </summary>
	MsmsecPrIeMatching = 0x00048009,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_SEC_IE_MATCHING
	/// Matching security capabilities of Secondary IE in M3 failed (RSN).
	/// </summary>
	MsmsecSecIeMatching = 0x0004800A,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_NO_PAIRWISE_KEY
	/// Required a pairwise key but Access Point configured only group keys.
	/// </summary>
	MsmsecNoPairwiseKey = 0x0004800B,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_G1_MISSING_KEY_DATA
	/// Message 1 of group key handshake has no key data (RSN/WPA).
	/// </summary>
	MsmsecG1MissingKeyData = 0x0004800C,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_G1_MISSING_GRP_KEY
	/// Message 1 of group key handshake has no group key.
	/// </summary>
	MsmsecG1MissingGrpKey = 0x0004800D,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PEER_INDICATED_INSECURE
	/// Access Point reset secure bit after connection was secured.
	/// </summary>
	MsmsecPeerIndicatedInsecure = 0x0004800E,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_NO_AUTHENTICATOR
	/// 802.1x indicated that there is no authenticator, but the profile requires 802.1x.
	/// </summary>
	MsmsecNoAuthenticator = 0x0004800F,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_NIC_FAILURE
	/// Plumbing settings to NIC failed.
	/// </summary>
	MsmsecNicFailure = 0x00048010,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_CANCELLED
	/// The operation was cancelled by caller.
	/// </summary>
	MsmsecCancelled = 0x00048011,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_KEY_FORMAT
	/// Entered key is not in valid format.
	/// </summary>
	MsmsecKeyFormat = 0x00048012,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_DOWNGRADE_DETECTED
	/// Security downgrade was detected.
	/// </summary>
	MsmsecDowngradeDetected = 0x00048013,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_PSK_MISMATCH_SUSPECTED
	/// PSK mismatch is suspected.
	/// </summary>
	MsmsecPskMismatchSuspected = 0x00048014,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_FORCED_FAILURE
	/// Forced failure because the connection method was not secure.
	/// </summary>
	MsmsecForcedFailure = 0x00048015,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_M3_TOO_MANY_RSNIE
	/// Message 3 of 4 way handshake contains too many RSN IE (RSN).
	/// </summary>
	MsmsecM3TooManyRsnie = 0x00048016,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_M2_MISSING_KEY_DATA
	/// Message 2 of 4 way handshake has no key data (RSN Adhoc).
	/// </summary>
	MsmsecM2MissingKeyData = 0x00048017,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_M2_MISSING_IE
	/// Message 2 of 4 way handshake has no IE (RSN Adhoc).
	/// </summary>
	MsmsecM2MissingIe = 0x00048018,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_AUTH_WCN_COMPLETED
	/// WCN authentication is completed as expected.
	/// </summary>
	MsmsecAuthWcnCompleted = 0x00048019,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_M3_MISSING_MGMT_GRP_KEY
	/// Message 3 of 4 way handshake has no group management key (RSN).
	/// </summary>
	MsmsecM3MissingMgmtGrpKey = 0x0004801A,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_G1_MISSING_MGMT_GRP_KEY
	/// Message 1 of group key handshake has no group management key.
	/// </summary>
	MsmsecG1MissingMgmtGrpKey = 0x0004801B,
	/// <summary>
	/// WLAN_REASON_CODE_MSMSEC_MAX
	/// </summary>
	MsmsecMax = 0x0004FFFF
}