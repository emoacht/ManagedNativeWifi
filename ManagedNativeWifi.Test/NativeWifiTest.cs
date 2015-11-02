using System;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ManagedNativeWifi.Test
{
	[TestClass]
	public class NativeWifiTest
	{
		private static Guid _interfaceGuid;
		private const string _testProfileName = "TestProfile";
		private const string _testSsidString = "TestSsid";

		[ClassInitialize]
		public static void Initialize(TestContext context)
		{
			_interfaceGuid = NativeWifi.EnumerateInterfaceGuids()
				.FirstOrDefault();
		}

		[TestMethod]
		public void SetProfileTest()
		{
			Assert.IsTrue(_interfaceGuid != null, "No wireless interface is connected.");

			var profileXml = CreateProfileXml(_testProfileName, _testSsidString);

			var result = NativeWifi.SetProfile(_interfaceGuid, ProfileType.AllUser, profileXml, null, true);
			Assert.IsTrue(result, "Failed to set the wireless profile for test.");

			Assert.IsTrue(NativeWifi.EnumerateProfileNames().Contains(_testProfileName),
				"The wireless profile for test doesn't appear.");
		}

		[TestMethod]
		public void DeletProfileTest()
		{
			Assert.IsTrue(_interfaceGuid != null, "No wireless interface is connected.");

			Assert.IsTrue(NativeWifi.EnumerateProfileNames().Contains(_testProfileName),
				"The wireless profile for test doesn't exist.");

			var result = NativeWifi.DeleteProfile(_interfaceGuid, _testProfileName);
			Assert.IsTrue(result, "Failed to delete the wireless profile for test.");

			Assert.IsFalse(NativeWifi.EnumerateProfileNames().Contains(_testProfileName),
				"The wireless profile for test remains.");
		}

		#region Helper

		private static string CreateProfileXml(string profileName, string ssidString)
		{
			if (string.IsNullOrWhiteSpace(profileName))
				throw new ArgumentNullException(nameof(profileName));

			if (string.IsNullOrWhiteSpace(ssidString))
				throw new ArgumentNullException(nameof(ssidString));

			return $@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
	<name>{profileName}</name>
	<SSIDConfig>
		<SSID>
			<hex>{ConvertFromStringToHexadecimalString(ssidString)}</hex>
			<name>{ssidString}</name>
		</SSID>
	</SSIDConfig>
	<connectionType>ESS</connectionType>
	<connectionMode>auto</connectionMode>
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
		}

		private static string ConvertFromStringToHexadecimalString(string source)
		{
			var buff = Encoding.UTF8.GetBytes(source);
			return BitConverter.ToString(buff).Replace("-", "");
		}

		#endregion
	}
}