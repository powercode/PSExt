using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PSExt {
	public sealed class PSSession : IDisposable {
		private readonly IProgram _program;
		private static PSSession _theSession;
		private readonly Runspace _runspace;
		private readonly AutoResetEvent _pipelineDoneEvent = new AutoResetEvent(false);

		public PSSession(IDebugger debugger, IProgram program) {
			_program = program;
			var initialSessionState = InitialSessionState.CreateDefault();
			initialSessionState.Commands.Add(GetInitialCommands());
			initialSessionState.Variables.Add(new SessionStateVariableEntry("Debugger", debugger, "Interface to the Windows debuggers", ScopedItemOptions.Constant));
			var host = new DbgPsHost(debugger, program);
			_runspace = RunspaceFactory.CreateRunspace(host, initialSessionState);
		}

		public static int Initialize(IDebugger debugger, IProgram program) {
			_theSession = new PSSession(debugger, program);
			return 0;
		}

		public static void Uninitialize() {
			_theSession.Dispose();
		}

		public static void InvokeCommand(string command) {
			_theSession.Invoke(command);
		}

		private void Invoke(string command) {
			if (_runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen)
			{
				_runspace.Open();
			}
			_pipelineDoneEvent.Reset();
			var pipeTask = Task.Factory.StartNew(() => InvokePipeline(command));
			_program.ProcessEvents(_pipelineDoneEvent);
			pipeTask.Wait();
		}

		void InvokePipeline(string command) {
			if (_runspace.RunspaceStateInfo.State == RunspaceState.BeforeOpen) {
				_runspace.Open();
			}
			var ps = PowerShell.Create();
			
			try
			{
				ps.Runspace = _runspace;
				ps.AddScript(command);
				ps.AddCommand(new CmdletInfo("Out-Default", typeof (Microsoft.PowerShell.Commands.OutDefaultCommand)));
				ps.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
				ps.Invoke();
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}
			finally {
				ps.Dispose();
				_pipelineDoneEvent.Set();
			}			
			
		}

		static IEnumerable<SessionStateCommandEntry> GetInitialCommands() {
			yield return new SessionStateCmdletEntry("Invoke-DbgCommand", typeof(InvokeDbgCommand), "");
			yield return new SessionStateCmdletEntry("Get-DbgBreakpoint", typeof(GetDbgBreakpointCommand), "");
			yield return new SessionStateCmdletEntry("Get-DbgModule", typeof(GetDebuggerModuleCommand), "");
			yield return new SessionStateAliasEntry("idc", "Invoke-DbgCommand");
		}

		public void Dispose() {
			_runspace.Close();
		}
	}
}