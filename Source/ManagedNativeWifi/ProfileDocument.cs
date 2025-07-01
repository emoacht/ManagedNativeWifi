﻿using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using ManagedNativeWifi.Common;

namespace ManagedNativeWifi;

/// <summary>
/// Container of wireless profile document
/// </summary>
/// <remarks>
/// The elements of profile XML are defined as:
/// https://docs.microsoft.com/en-us/windows/win32/nativewifi/wlan-profileschema-wlanprofile-element
/// </remarks>
public class ProfileDocument
{
	/// <summary>
	/// Target namespace of profile XML
	/// </summary>
	protected const string Namespace = @"http://www.microsoft.com/networking/WLAN/profile/v1";

	/// <summary>
	/// Root element
	/// </summary>
	protected XDocument Root { get; }

	/// <summary>
	/// Profile name
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// SSID of associated wireless LAN
	/// </summary>
	public NetworkIdentifier Ssid { get; }

	/// <summary>
	/// BSS network type of associated wireless LAN
	/// </summary>
	public BssType BssType { get; }

	/// <summary>
	/// Authentication method of associated wireless LAN
	/// </summary>
	public AuthenticationMethod Authentication { get; }
	internal string AuthenticationString { get; }

	/// <summary>
	/// Encryption type of associated wireless LAN
	/// </summary>
	public EncryptionType Encryption { get; }
	internal string EncryptionString { get; }

	private XElement _connectionModeElement;
	private XElement _autoSwitchElement;

	/// <summary>
	/// Whether profile XML is valid
	/// </summary>
	public virtual bool IsValid { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="xml">Profile XML</param>
	public ProfileDocument(string xml) : this(Parse(xml))
	{ }

	private ProfileDocument(XDocument root)
	{
		if (root is null)
			return;

		this.Root = root;

		Name = Root.Elements().First().Elements(XName.Get("name", Namespace)).FirstOrDefault()?.Value;

		var ssidElement = Root.Descendants(XName.Get("SSID", Namespace)).FirstOrDefault();
		var ssidHexString = ssidElement?.Descendants(XName.Get("hex", Namespace)).FirstOrDefault()?.Value;
		var ssidHexBytes = HexadecimalHelper.ToBytes(ssidHexString);
		var ssidNameString = ssidElement?.Descendants(XName.Get("name", Namespace)).FirstOrDefault()?.Value;
		Ssid = new NetworkIdentifier(ssidHexBytes, ssidNameString);

		int count = 0;

		var connectionTypeString = Root.Descendants(XName.Get("connectionType", Namespace)).FirstOrDefault()?.Value;
		if (BssTypeConverter.TryParse(connectionTypeString, out BssType bssType)) count++;
		this.BssType = bssType;

		AuthenticationString = Root.Descendants(XName.Get("authentication", Namespace)).FirstOrDefault()?.Value;
		if (AuthenticationMethodConverter.TryParse(AuthenticationString, out AuthenticationMethod authentication)) count++;
		this.Authentication = authentication;

		EncryptionString = Root.Descendants(XName.Get("encryption", Namespace)).FirstOrDefault()?.Value;
		if (EncryptionTypeConverter.TryParse(EncryptionString, out EncryptionType encryption)) count++;
		this.Encryption = encryption;

		//Debug.WriteLine("SSID: {0}, BssType: {1}, Authentication: {2}, Encryption: {3}",
		//	Ssid,
		//	BssType,
		//	Authentication,
		//	Encryption);

		_connectionModeElement = Root.Descendants(XName.Get("connectionMode", Namespace)).FirstOrDefault();
		if (_connectionModeElement is not null) count++;

		_autoSwitchElement = Root.Descendants(XName.Get("autoSwitch", Namespace)).FirstOrDefault();

		IsValid = (count == 4);
	}

	private static XDocument Parse(string xml)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(xml))
				throw new ArgumentNullException(nameof(xml));

			try
			{
				var document = XDocument.Parse(xml);

				if (!string.Equals(document.Root.Name.NamespaceName, Namespace))
					throw new ArgumentException("The namespace does not indicate a profile.", nameof(xml));

				return document;
			}
			catch (XmlException ex)
			{
				throw new ArgumentException(ex.Message, nameof(xml), ex);
			}
		}
		catch
		{
#if DEBUG
			throw;
#else
			return null;
#endif
		}
	}

	private enum ConnectionMode { Auto, Manual }

	/// <summary>
	/// Whether automatic connection for this profile is enabled
	/// </summary>
	public bool IsAutoConnectEnabled
	{
		get
		{
			return Enum.TryParse(_connectionModeElement?.Value, true, out ConnectionMode value)
				&& (value is ConnectionMode.Auto);
		}
		set
		{
			if (value && (BssType is not BssType.Infrastructure))
				return;

			if (_connectionModeElement is null)
				return;

			_connectionModeElement.Value = (value ? ConnectionMode.Auto : ConnectionMode.Manual).ToString().ToLower();

			// If automatic connection is disabled, automatic switch will be disabled as well. 
			if (!value)
				IsAutoSwitchEnabled = false;
		}
	}

	/// <summary>
	/// Whether automatic switch for this profile is enabled
	/// </summary>
	public bool IsAutoSwitchEnabled
	{
		get => ((bool?)_autoSwitchElement).GetValueOrDefault();
		set
		{
			if (value && !IsAutoConnectEnabled)
				return;

			if (_autoSwitchElement is null)
			{
				if (_connectionModeElement is null)
					return;

				_autoSwitchElement = new XElement(XName.Get("autoSwitch", Namespace));
				_connectionModeElement.AddAfterSelf(_autoSwitchElement);
			}
			_autoSwitchElement.Value = value.ToString().ToLower();
		}
	}

	/// <summary>
	/// Profile XML
	/// </summary>
	public string Xml => $"{Root?.Declaration}{Environment.NewLine}{Root?.ToString()}";

	/// <summary>
	/// Creates new instance cloned from this instance by deep copy.
	/// </summary>
	/// <returns>Cloned instance</returns>
	public virtual ProfileDocument Clone() => new ProfileDocument(new XDocument(Root));
}