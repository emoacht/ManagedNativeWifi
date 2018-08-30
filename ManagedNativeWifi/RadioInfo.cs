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
		public IReadOnlyList<RadioSet> RadioSets => Array.AsReadOnly(_radioSets);
		private readonly RadioSet[] _radioSets;

		/// <summary>
		/// Constructor
		/// </summary>
		public RadioInfo(Guid id, IEnumerable<RadioSet> radioSets)
		{
			this.Id = id;
			this._radioSets = radioSets.ToArray();
		}
	}
}