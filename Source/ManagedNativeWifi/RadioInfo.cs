using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi
{
	/// <summary>
	/// Wireless interface radio information
	/// </summary>
	public class RadioInfo
	{
		/// <summary>
		/// Interface ID
		/// </summary>
		public Guid Id { get; }

		/// <summary>
		/// Radio information
		/// </summary>
		public IReadOnlyList<RadioSet> RadioSets { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		public RadioInfo(Guid id, IEnumerable<RadioSet> radioSets)
		{
			this.Id = id;
			this.RadioSets = Array.AsReadOnly(radioSets?.ToArray() ?? new RadioSet[0]);
		}
	}
}