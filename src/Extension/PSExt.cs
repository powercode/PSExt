using System;
using System.Runtime.InteropServices;
using PSExt.Host;
using RGiesecke.DllExport;

namespace PSExt.Extension
{
	public partial class DebuggerExtension
	{
		private static IDebugger _debugger;
		private static IDebugger Debugger => _debugger ?? (_debugger = new DebuggerProxy(new Debugger(DebugClient), Dispatcher));
		
		private static DebuggerDispatcher _dispatcher;
		private static DebuggerDispatcher Dispatcher => _dispatcher ?? (_dispatcher = new DebuggerDispatcher());
		
		private static ExitManager _exitManager;
		private static ExitManager ExitManager => _exitManager ?? (_exitManager = new ExitManager());

		private static PSSession _powerShellSession;
		private static PSSession PowerShellSession => _powerShellSession ??
		                                             (_powerShellSession = new PSSession(Debugger, new DbgPsHost(Debugger, ExitManager), Dispatcher));

		[DllExport("ps")]
		public static void PS(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string args)
		{
			// Must be the first thing in our extension.
			if (!InitApi(client))
				return;
			
			PowerShellSession.Invoke(args);						
		}

	}
}
