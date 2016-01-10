using System;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Interop;
using PSExt.Commands;
using RGiesecke.DllExport;

namespace PSExt.Extension
{
	public partial class DebuggerExtension
	{
		private static IDebugger _debugger;
		private static IDebugger Debugger => _debugger ?? (_debugger = new DebuggerProxy(new Debugger(DebugClient), Dispatcher));
		
		private static DebuggerDispatcher _dispatcher;
		private static DebuggerDispatcher Dispatcher => _dispatcher ?? (_dispatcher = new DebuggerDispatcher());
		
		private static Program _program;
		private static Program Program => _program ?? (_program = new Program(Debugger, Dispatcher));

		private static PSSession _powerShellSession;

		private static PSSession PowerShellSession => _powerShellSession ??
		                                             (_powerShellSession = new PSSession(Debugger, Program));

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
