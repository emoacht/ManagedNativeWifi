using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Container of wireless profile document
	/// </summary>
	/// <remarks>
	/// The elements of profile XML are defined as:
	/// https://msdn.microsoft.com/en-us/library/windows/desktop/ms707381.aspx
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
			if (root == null)
				return;

			this.Root = root;

			Name = Root.Elements().First().Elements(XName.Get("name", Namespace)).FirstOrDefault()?.Value;

			var ssidElement = Root.Descendants(XName.Get("SSID", Namespace)).FirstOrDefault();
			var ssidHexString = ssidElement?.Descendants(XName.Get("hex", Namespace)).FirstOrDefault()?.Value;
			var ssidHexBytes = HexadecimalStringConverter.ToBytes(ssidHexString);
			var ssidNameString = ssidElement?.Descendants(XName.Get("name", Namespace)).FirstOrDefault()?.Value;
			Ssid = new NetworkIdentifier(ssidHexBytes, ssidNameString);

			var connectionTypeString = Root.Descendants(XName.Get("connectionType", Namespace)).FirstOrDefault()?.Value;
			BssType = BssTypeConverter.Parse(connectionTypeString);

			AuthenticationString = Root.Descendants(XName.Get("authentication", Namespace)).FirstOrDefault()?.Value;
			Authentication = AuthenticationMethodConverter.ToAuthenticationMethod(AuthenticationString);

			EncryptionString = Root.Descendants(XName.Get("encryption", Namespace)).FirstOrDefault()?.Value;
			Encryption = EncryptionTypeConverter.ToEncryptionType(EncryptionString);

			//Debug.WriteLine("SSID: {0}, BssType: {1}, Authentication: {2}, Encryption: {3}",
			//	Ssid,
			//	BssType,
			//	Authentication,
			//	Encryption);

			_connectionModeElement = Root.Descendants(XName.Get("connectionMode", Namespace)).FirstOrDefault();
			_autoSwitchElement = Root.Descendants(XName.Get("autoSwitch", Namespace)).FirstOrDefault();

			IsValid = (_connectionModeElement != null);
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
		public bool IsAutoConnectionEnabled
		{
			get
			{
				return Enum.TryParse(_connectionModeElement?.Value, true, out ConnectionMode value)
					&& (value == ConnectionMode.Auto);
			}
			set
			{
				if (_connectionModeElement == null)
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
				if (_autoSwitchElement == null)
				{
					if (_connectionModeElement == null)
						return;

					_autoSwitchElement = new XElement(XName.Get("autoSwitch", Namespace));
					_connectionModeElement.AddAfterSelf(_autoSwitchElement);
				}
				_autoSwitchElement.Value = value.ToString().ToLower();
			}
		}

		/// <summary>
		/// Returns profile XML.
		/// </summary>
		/// <returns>Profile XML</returns>
		public override string ToString() => Root?.Declaration + Root?.ToString();

		/// <summary>
		/// Creates new instance cloned from this instance by deep copy.
		/// </summary>
		/// <returns>Cloned instance</returns>
		public virtual ProfileDocument Clone() => new ProfileDocument(new XDocument(Root));
	}

	internal static class HexadecimalStringConverter
	{
		/// <summary>
		/// Converts string which represents a byte array in hexadecimal format to the byte array.
		/// </summary>
		/// <param name="source">Hexadecimal string</param>
		/// <returns>Original byte array</returns>
		public static byte[] ToBytes(string source)
		{
			if (string.IsNullOrWhiteSpace(source))
				return null;

			var buff = new byte[source.Length / 2];

			for (int i = 0; i < buff.Length; i++)
			{
				try
				{
					buff[i] = Convert.ToByte(source.Substring(i * 2, 2), 16);
				}
				catch (FormatException)
				{
					break;
				}
			}
			return buff;
		}

		/// <summary>
		/// Converts a byte array to string which represents the byte array in hexadecimal format.
		/// </summary>
		/// <param name="source">Original byte array</param>
		/// <returns>Hexadecimal string</returns>
		public static string ToHexadecimalString(byte[] source) =>
			BitConverter.ToString(source).Replace("-", "");

		public static string ToHexadecimalString(string source) =>
			ToHexadecimalString(Encoding.UTF8.GetBytes(source));
	}
}