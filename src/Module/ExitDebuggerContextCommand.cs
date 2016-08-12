using System;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;

namespace PSExt.Commands
{
	[Cmdlet(VerbsCommon.Exit, "DebugContext")]
	public class ExitDebuggerContextCommand : DbgBaseCmdlet
	{
		static readonly IntPtr PseudoProcess = new IntPtr(4711);
		protected override void EndProcessing()
		{
			var currentPowerShell = PowerShell.Create(RunspaceMode.CurrentRunspace);			
			var pseudoProcess = currentPowerShell.Runspace.InstanceId.GetHashCode();		
			Debugger.EndDumpSession();
			Debugger.Dispose();
			SymCleanup(new IntPtr(pseudoProcess));
			SessionState.PSVariable.Set("Debugger", null);
		}
		
				
		[DllImport("dbghelp.dll", EntryPoint = "SymCleanup", CallingConvention = CallingConvention.StdCall)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SymCleanup([In] IntPtr hProcess);		

	}
}