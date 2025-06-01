using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagedNativeWifi;

/// <summary>
/// Wireless interface radio information
/// </summary>
public class RadioInfo
{
	/// <summary>
	/// Interface ID
	/// </summary>
	public Guid InterfaceId { get; }

	/// <summary>
	/// Radio information
	/// </summary>
	public IReadOnlyList<RadioSet> RadioSets { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	public RadioInfo(Guid interfaceId, IEnumerable<RadioSet> radioSets)
	{
		this.InterfaceId = interfaceId;
		this.RadioSets = Array.AsReadOnly(radioSets?.ToArray() ?? []);
	}
}