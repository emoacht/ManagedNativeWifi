using System;
using System.Text;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ManagedNativeWifi.Test
{
	[TestClass]
	public class NativeWifiTest
	{
		#region SetProfile/DeleteProfile

		private static Guid _interfaceId;
		private const string _testProfileName = "TestProfile";
		private const string _testSsidString = "TestSsid";

		[ClassInitialize]
		public static void Initialize(TestContext context)
		{
			_interfaceId = NativeWifi.EnumerateInterfaces()
				.Select(x => x.Id)
				.FirstOrDefault();
		}

		[TestMethod]
		public void SetProfileTest()
		{
			Assert.IsTrue(_interfaceId != null, "No wireless interface is connected.");

			var profileXml = CreateProfileXml(_testProfileName, _testSsidString);

			var result = NativeWifi.SetProfile(_interfaceId, ProfileType.AllUser, profileXml, null, true);
			Assert.IsTrue(result, "Failed to set the wireless profile for test.");

			Assert.IsTrue(NativeWifi.EnumerateProfileNames().Contains(_testProfileName),
				"The wireless profile for test doesn't appear.");
		}

		[TestMethod]
		public void DeletProfileTest()
		{
			Assert.IsTrue(_interfaceId != null, "No wireless interface is connected.");

			Assert.IsTrue(NativeWifi.EnumerateProfileNames().Contains(_testProfileName),
				"The wireless profile for test doesn't exist.");

			var result = NativeWifi.DeleteProfile(_interfaceId, _testProfileName);
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

		#endregion

		#region Channel

		[TestMethod]
		public void DetectChannel24000Test()
		{
			// Valid cases
			Assert.AreEqual(1, NativeWifi.DetectChannel(2412000));
			Assert.AreEqual(2, NativeWifi.DetectChannel(2417000));
			Assert.AreEqual(5, NativeWifi.DetectChannel(2432000));
			Assert.AreEqual(6, NativeWifi.DetectChannel(2437000));
			Assert.AreEqual(12, NativeWifi.DetectChannel(2467000));
			Assert.AreEqual(13, NativeWifi.DetectChannel(2472000));
			Assert.AreEqual(14, NativeWifi.DetectChannel(2484000));

			// Invalid cases
			Assert.AreEqual(0, NativeWifi.DetectChannel(2411000));
			Assert.AreEqual(0, NativeWifi.DetectChannel(2485000));
			Assert.AreEqual(0, NativeWifi.DetectChannel(2453000));
		}

		[TestMethod]
		public void DetectChannel36000Test()
		{
			// Valid cases
			Assert.AreEqual(131, NativeWifi.DetectChannel(3657500));
			Assert.AreEqual(132, NativeWifi.DetectChannel(3660000));
			Assert.AreEqual(132, NativeWifi.DetectChannel(3662500));
			Assert.AreEqual(133, NativeWifi.DetectChannel(3665000));
			Assert.AreEqual(133, NativeWifi.DetectChannel(3667500));
			Assert.AreEqual(135, NativeWifi.DetectChannel(3675000));
			Assert.AreEqual(135, NativeWifi.DetectChannel(3677500));
			Assert.AreEqual(138, NativeWifi.DetectChannel(3690000));
			Assert.AreEqual(138, NativeWifi.DetectChannel(3692500));

			// Invalid cases
			Assert.AreEqual(0, NativeWifi.DetectChannel(3657000));
			Assert.AreEqual(0, NativeWifi.DetectChannel(3695000));
			Assert.AreEqual(0, NativeWifi.DetectChannel(3673000));
		}

		[TestMethod]
		public void DetectChannel50000Test()
		{
			// Valid cases
			Assert.AreEqual(34, NativeWifi.DetectChannel(5170000));
			Assert.AreEqual(36, NativeWifi.DetectChannel(5180000));
			Assert.AreEqual(40, NativeWifi.DetectChannel(5200000));
			Assert.AreEqual(52, NativeWifi.DetectChannel(5260000));
			Assert.AreEqual(56, NativeWifi.DetectChannel(5280000));
			Assert.AreEqual(140, NativeWifi.DetectChannel(5700000));
			Assert.AreEqual(144, NativeWifi.DetectChannel(5720000));
			Assert.AreEqual(149, NativeWifi.DetectChannel(5745000));
			Assert.AreEqual(151, NativeWifi.DetectChannel(5755000));
			Assert.AreEqual(161, NativeWifi.DetectChannel(5805000));
			Assert.AreEqual(165, NativeWifi.DetectChannel(5825000));

			// Invalid cases
			Assert.AreEqual(0, NativeWifi.DetectChannel(5160000));
			Assert.AreEqual(0, NativeWifi.DetectChannel(5850000));
			Assert.AreEqual(0, NativeWifi.DetectChannel(5651000));
		}

		#endregion
	}
}