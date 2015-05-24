using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedNativeWifi
{
	class Program
	{
		static void Main(string[] args)
		{
			if (!Debugger.IsAttached)
				Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

			Debug.WriteLine("[Available Networks]");
			NativeWifi.GetAvailableNetworkSsid().ToArray();

			Debug.WriteLine("[Connected Networks]");
			NativeWifi.GetConnectedNetworkSsid().ToArray();
		}
	}
}