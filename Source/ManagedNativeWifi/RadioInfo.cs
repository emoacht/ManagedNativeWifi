using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagedNativeWifi;

/// <summary>
/// Radio information
/// </summary>
public class RadioInfo
{
	/// <summary>
	/// Radio state information
	/// </summary>
	public IReadOnlyList<RadioStateSet> RadioStates { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	public RadioInfo(IEnumerable<RadioStateSet> radioStates)
	{
		this.RadioStates = Array.AsReadOnly(radioStates?.ToArray() ?? []);
	}
}