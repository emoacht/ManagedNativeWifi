using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Wireless radio information
	/// </summary>
	public class RadioSet
	{
		/// <summary>
		/// 802.11 PHY and media type
		/// </summary>
		public PhyType Type { get; }

		/// <summary>
		/// Whether hardware radio state is on
		/// </summary>
		public bool? HardwareOn { get; }

		/// <summary>
		/// Whether software radio state is on
		/// </summary>
		public bool? SoftwareOn { get; }

		/// <summary>
		/// Whether the radio is on
		/// </summary>
		public bool? On => (HardwareOn.HasValue && SoftwareOn.HasValue)
			? HardwareOn.Value && SoftwareOn.Value
			: (bool?)null;

		/// <summary>
		/// Constructor
		/// </summary>
		public RadioSet(PhyType type, bool? hardwareOn, bool? softwareOn)
		{
			this.Type = type;
			this.HardwareOn = hardwareOn;
			this.SoftwareOn = softwareOn;
		}
	}
}