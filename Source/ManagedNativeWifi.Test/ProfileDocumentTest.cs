using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ManagedNativeWifi.Test
{
	[TestClass]
	public class ProfileDocumentTest
	{
		private readonly string _testProfileName = $"TestProfile{DateTime.Today.Year}";
		private readonly string _testSsidString = $"TestSsid{DateTime.Today.DayOfYear}";

		[TestMethod]
		public void LoadProfileTest1()
		{
			var document = new ProfileDocument(CreateProfile1(_testProfileName, _testSsidString));

			Assert.IsTrue(document.Name == _testProfileName, "Failed to get wireless profile name.");
			Assert.IsTrue(document.Ssid.ToString() == _testSsidString, "Failed to get wireless profile SSID.");
			Assert.IsTrue(document.BssType == BssType.Infrastructure, "Failed to get wireless profile BSS type.");
			Assert.IsTrue(document.Authentication == AuthenticationMethod.WPA_Personal, "Failed to get wireless profile authentication.");
			Assert.IsTrue(document.Encryption == EncryptionType.TKIP, "Failed to get wireless profile encryption.");
			Assert.IsTrue(!document.IsAutoConnectEnabled, "Failed to get wireless profile automatic connection.");
			Assert.IsTrue(!document.IsAutoSwitchEnabled, "Failed to get wireless profile automatic switch.");

			document.IsAutoConnectEnabled = true;
			Assert.IsTrue(document.IsAutoConnectEnabled, "Failed to enable wireless profile automatic connection.");

			document.IsAutoSwitchEnabled = true;
			Assert.IsTrue(document.IsAutoSwitchEnabled, "Failed to enable wireless profile automatic switch.");

			document.IsAutoSwitchEnabled = false;
			Assert.IsTrue(!document.IsAutoSwitchEnabled, "Failed to disable wireless profile automatic switch.");
		}

		[TestMethod]
		public void LoadProfileTest2()
		{
			var document = new ProfileDocument(CreateProfile2(_testProfileName, _testSsidString));

			Assert.IsTrue(document.BssType == BssType.Infrastructure, "Failed to get wireless profile BSS type.");
			Assert.IsTrue(document.Authentication == AuthenticationMethod.WPA2_Personal, "Failed to get wireless profile authentication.");
			Assert.IsTrue(document.Encryption == EncryptionType.AES, "Failed to get wireless profile encryption.");
			Assert.IsTrue(document.IsAutoConnectEnabled, "Failed to get wireless profile automatic connection.");
			Assert.IsTrue(document.IsAutoSwitchEnabled, "Failed to get wireless profile automatic switch.");

			document.IsAutoConnectEnabled = false;
			Assert.IsTrue(!document.IsAutoConnectEnabled, "Failed to disable wireless profile automatic connection.");
			Assert.IsTrue(!document.IsAutoSwitchEnabled, "Failed to disable wireless profile automatic switch.");

			document.IsAutoSwitchEnabled = true;
			Assert.IsTrue(!document.IsAutoSwitchEnabled, "Failed to interrupt enabling wireless profile automatic switch.");
		}

		[TestMethod]
		public void LoadProfileTest3()
		{
			var document = new ProfileDocument(CreateProfile3(_testProfileName, _testSsidString));

			Assert.IsTrue(document.BssType == BssType.Independent, "Failed to get wireless profile BSS type.");
			Assert.IsTrue(document.Authentication == AuthenticationMethod.Open, "Failed to get wireless profile authentication.");
			Assert.IsTrue(document.Encryption == EncryptionType.None, "Failed to get wireless profile encryption.");
			Assert.IsTrue(!document.IsAutoConnectEnabled, "Failed to get wireless profile automatic connection.");
			Assert.IsTrue(!document.IsAutoSwitchEnabled, "Failed to get wireless profile automatic switch.");

			document.IsAutoConnectEnabled = true;
			Assert.IsTrue(!document.IsAutoConnectEnabled, "Failed to interrupt enabling wireless profile automatic connection.");

			document.IsAutoSwitchEnabled = true;
			Assert.IsTrue(!document.IsAutoSwitchEnabled, "Failed to interrupt enabling wireless profile automatic switch.");
		}

		#region Helper

		private static string CreateProfile1(string profileName, string ssidString) =>
			$@"<?xml version=""1.0"" encoding=""US-ASCII""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
	<name>{profileName}</name>
	<SSIDConfig>
		<SSID>
			<hex>{HexadecimalStringConverter.ToHexadecimalString(ssidString)}</hex>
			<name>{ssidString}</name>
		</SSID>
	</SSIDConfig>
	<connectionType>ESS</connectionType>
	<connectionMode>manual</connectionMode>
	<MSM>
		<security>
			<authEncryption>
				<authentication>WPAPSK</authentication>
				<encryption>TKIP</encryption>
				<useOneX>false</useOneX>
			</authEncryption>
		</security>
	</MSM>
</WLANProfile>";

		private static string CreateProfile2(string profileName, string ssidString) =>
			$@"<?xml version=""1.0"" encoding=""US-ASCII""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
	<name>{profileName}</name>
	<SSIDConfig>
		<SSID>
			<hex>{HexadecimalStringConverter.ToHexadecimalString(ssidString)}</hex>
			<name>{ssidString}</name>
		</SSID>
	</SSIDConfig>
	<connectionType>ESS</connectionType>
	<connectionMode>auto</connectionMode>
	<autoSwitch>true</autoSwitch>
	<MSM>
		<security>
			<authEncryption>
				<authentication>WPA2PSK</authentication>
				<encryption>AES</encryption>
				<useOneX>false</useOneX>
			</authEncryption>
		</security>
	</MSM>
</WLANProfile>";

		private static string CreateProfile3(string profileName, string ssidString) =>
	$@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
	<name>{profileName}</name>
	<SSIDConfig>
		<SSID>
			<hex>{HexadecimalStringConverter.ToHexadecimalString(ssidString)}</hex>
			<name>{ssidString}</name>
		</SSID>
	</SSIDConfig>
	<connectionType>IBSS</connectionType>
	<connectionMode>manual</connectionMode>
	<autoSwitch>false</autoSwitch>
	<MSM>
		<security>
			<authEncryption>
				<authentication>open</authentication>
				<encryption>none</encryption>
				<useOneX>false</useOneX>
			</authEncryption>
		</security>
	</MSM>
</WLANProfile>";

		#endregion
	}
}