using System;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Interop;

namespace PSExt.Commands
{
	[Cmdlet(VerbsCommon.Enter, "DebugContext")]		
	public class EnterDebuggerContextCommand : PSCmdlet
	{		
		[Parameter(Mandatory = true)]
		public string DumpPath { get; set; }

		[Parameter(Mandatory = true)]
		public string SymbolPath { get; set; }

		protected override void EndProcessing()
		{
			ProviderInfo provider;
			var paths = this.SessionState.Path.GetResolvedProviderPathFromPSPath(DumpPath, out provider);
			if (provider.Name != "FileSystem")
			{
				ThrowTerminatingError(new ErrorRecord(new Exception("Not a filesystem path"), "DumpPathNotInFileSystem", ErrorCategory.InvalidArgument, DumpPath));				
			}
			if (paths.Count > 1)
			{
				ThrowTerminatingError(new ErrorRecord(new Exception("MultipleFiles"), "DumpPathResolvesToMultipleFiles", ErrorCategory.InvalidArgument, DumpPath));
			}			
			var debugClient5Uuid = new Guid("e3acb9d7-7ec2-4f0c-a0da-e81e0cbbe628");
			
			object pObj ;
			var res = DebugCreate(ref debugClient5Uuid, out pObj);
			if (res != 0)
			{
				ThrowTerminatingError(new ErrorRecord(new Win32Exception(res), "DebugCreateFailure", ErrorCategory.InvalidArgument, DumpPath));			
			}
			var client = (IDebugClient5)pObj;
			var sym = (IDebugSymbols)client;
			var currentPowerShell = PowerShell.Create(RunspaceMode.CurrentRunspace);
			var id = currentPowerShell.Runspace.InstanceId.GetHashCode();
						
			SymInitialize(new IntPtr(id), SymbolPath, false);
			if (!String.IsNullOrEmpty(SymbolPath))
			{				
				sym.SetSymbolPath(SymbolPath);
				sym.SetImagePath(SymbolPath);
			}
			
			res = client.OpenDumpFileWide(DumpPath, 0);

			var control = (IDebugControl4)client;
			control.WaitForEvent(DEBUG_WAIT.DEFAULT, UInt32.MaxValue);			
			if (res != 0)
			{
				ThrowTerminatingError(new ErrorRecord(new Win32Exception(res), "OpenDumpFile", ErrorCategory.InvalidArgument, DumpPath ));
			}
			if (!String.IsNullOrEmpty(SymbolPath))
			{
				sym.SetSymbolPath(SymbolPath);
				sym.SetImagePath(SymbolPath);
			}
			SessionState.PSVariable.Set("Debugger", new Debugger(client));
		}
		
		[DllImport("dbgeng.dll", EntryPoint = "DebugCreate", CallingConvention = CallingConvention.StdCall)]
		private static extern int DebugCreate([In] ref System.Guid interfaceId, [MarshalAs(UnmanagedType.IUnknown)] out object debugClient);

		[DllImport("dbghelp.dll", EntryPoint = "SymInitialize", CallingConvention = CallingConvention.StdCall)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SymInitialize([In] IntPtr hProcess, string userSearchPath, bool invadeProcess);
		
	}

	[Cmdlet(VerbsCommon.Set, "SymbolPath")]	
	public class SetSymbolPathCommand : DbgBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 1)]
		public string SymbolPath{ get; set; }
		protected override void ProcessRecord()
		{
			Debugger.SetSymbolPath(SymbolPath);			
		}
	}

	[Cmdlet(VerbsData.Update, "Symbols")]
	public class UpdateSymbolCommand : DbgBaseCmdlet
	{		
		protected override void ProcessRecord()
		{
			Debugger.ReloadSymbols();
		}
	}
}