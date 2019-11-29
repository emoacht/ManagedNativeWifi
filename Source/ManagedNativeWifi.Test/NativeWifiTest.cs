using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ManagedNativeWifi.Test
{
	[TestClass]
	public class NativeWifiTest
	{
		#region Set/Delete Profile

		private static Guid _interfaceId;

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
			var testProfileName = $"TestProfile{DateTime.Today.Year}";
			var testSsidString = $"TestSsidString{DateTime.Today.DayOfYear}";

			Assert.IsTrue(_interfaceId != null, "No wireless interface is connected.");

			var profileXml = CreateProfileXml(testProfileName, testSsidString);

			var result = NativeWifi.SetProfile(_interfaceId, ProfileType.AllUser, profileXml, null, true);
			Assert.IsTrue(result, "Failed to set the wireless profile for test.");

			Assert.IsTrue(NativeWifi.EnumerateProfileNames().Contains(testProfileName),
				"The wireless profile for test doesn't appear.");
		}

		[TestMethod]
		public void DeletProfileTest()
		{
			var testProfileName = $"TestProfile{DateTime.Today.Year}";
			var testSsidString = $"TestSsidString{DateTime.Today.DayOfYear}";

			Assert.IsTrue(_interfaceId != null, "No wireless interface is connected.");

			Assert.IsTrue(NativeWifi.EnumerateProfileNames().Contains(testProfileName),
				"The wireless profile for test doesn't exist.");

			var result = NativeWifi.DeleteProfile(_interfaceId, testProfileName);
			Assert.IsTrue(result, "Failed to delete the wireless profile for test.");

			Assert.IsFalse(NativeWifi.EnumerateProfileNames().Contains(testProfileName),
				"The wireless profile for test remains.");
		}

		#region Helper

		private static string CreateProfileXml(string profileName, string ssidString) =>
			$@"<?xml version=""1.0""?>
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

		#endregion

		#region Channel

		/// <summary>
		/// Detects channels of 2.4GHz.
		/// </summary>
		[TestMethod]
		public void TryDetectChannel24000Test()
		{
			// Valid cases
			TryDetectChannelValidTestBase(2_412_000, 2.4F, 1);
			TryDetectChannelValidTestBase(2_417_000, 2.4F, 2);
			TryDetectChannelValidTestBase(2_432_000, 2.4F, 5);
			TryDetectChannelValidTestBase(2_437_000, 2.4F, 6);
			TryDetectChannelValidTestBase(2_467_000, 2.4F, 12);
			TryDetectChannelValidTestBase(2_472_000, 2.4F, 13);
			TryDetectChannelValidTestBase(2_484_000, 2.4F, 14);

			// Invalid cases
			TryDetectChannelInvalidTestBase(2_411_000);
			TryDetectChannelInvalidTestBase(2_485_000);
			TryDetectChannelInvalidTestBase(2_453_000);
		}

		/// <summary>
		/// Detects channels of 3.6GHz.
		/// </summary>
		[TestMethod]
		public void TryDetectChannel36000Test()
		{
			// Valid cases			
			TryDetectChannelValidTestBase(3_657_500, 3.6F, 131);
			TryDetectChannelValidTestBase(3_660_000, 3.6F, 132);
			TryDetectChannelValidTestBase(3_662_500, 3.6F, 132);
			TryDetectChannelValidTestBase(3_665_000, 3.6F, 133);
			TryDetectChannelValidTestBase(3_667_500, 3.6F, 133);
			TryDetectChannelValidTestBase(3_675_000, 3.6F, 135);
			TryDetectChannelValidTestBase(3_677_500, 3.6F, 135);
			TryDetectChannelValidTestBase(3_690_000, 3.6F, 138);
			TryDetectChannelValidTestBase(3_692_500, 3.6F, 138);

			// Invalid cases
			TryDetectChannelInvalidTestBase(3_657_000);
			TryDetectChannelInvalidTestBase(3_695_000);
			TryDetectChannelInvalidTestBase(3_673_000);
		}

		/// <summary>
		/// Detects channels of 5GHz.
		/// </summary>
		[TestMethod]
		public void TryDetectChannel50000Test()
		{
			// Valid cases
			TryDetectChannelValidTestBase(5_170_000, 5F, 34);
			TryDetectChannelValidTestBase(5_180_000, 5F, 36);
			TryDetectChannelValidTestBase(5_200_000, 5F, 40);
			TryDetectChannelValidTestBase(5_260_000, 5F, 52);
			TryDetectChannelValidTestBase(5_280_000, 5F, 56);
			TryDetectChannelValidTestBase(5_700_000, 5F, 140);
			TryDetectChannelValidTestBase(5_720_000, 5F, 144);
			TryDetectChannelValidTestBase(5_745_000, 5F, 149);
			TryDetectChannelValidTestBase(5_755_000, 5F, 151);
			TryDetectChannelValidTestBase(5_805_000, 5F, 161);
			TryDetectChannelValidTestBase(5_825_000, 5F, 165);

			// Invalid cases
			TryDetectChannelInvalidTestBase(5_160_000);
			TryDetectChannelInvalidTestBase(5_850_000);
			TryDetectChannelInvalidTestBase(5_651_000);
		}

		private void TryDetectChannelValidTestBase(uint frequency, float expectedBand, int expectedChannel)
		{
			Assert.IsTrue(NativeWifi.TryDetectBandChannel(frequency, out var band, out var channel));
			Assert.AreEqual(expectedBand, band);
			Assert.AreEqual(expectedChannel, channel);
		}

		private void TryDetectChannelInvalidTestBase(uint frequency)
		{
			Assert.IsFalse(NativeWifi.TryDetectBandChannel(frequency, out _, out _));
		}

		#endregion
	}
}