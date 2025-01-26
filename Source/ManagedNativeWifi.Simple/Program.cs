using System;
using System.Diagnostics;
using System.Linq;

namespace ManagedNativeWifi.Simple;

class Program
{
	static void Main(string[] args)
	{
		if (!Debugger.IsAttached)
			Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

		Debug.WriteLine("[Available Network SSIDs]");
		NativeWifi.GetAvailableNetworkSsids().ToArray();

		Debug.WriteLine("[Connected Network SSIDs]");
		NativeWifi.GetConnectedNetworkSsids().ToArray();
	}
}