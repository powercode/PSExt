using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.PowerShell.Commands;

namespace PSExt
{
	public sealed class PSSession : IDisposable
	{
		private static PSSession _theSession;
		private PowerShell _currentPowerShell;
		private readonly DbgPsHost _host;
		private readonly object _instanceLock = new object();
		private readonly AutoResetEvent _pipelineDoneEvent = new AutoResetEvent(false);
		private readonly IProgram _program;
		private readonly Runspace _runspace;

		public PSSession(IDebugger debugger, IProgram program)
		{
			_program = program;
			var initialSessionState = InitialSessionState.CreateDefault();
			initialSessionState.Commands.Add(GetInitialCommands());
			initialSessionState.Variables.Add(new SessionStateVariableEntry("Debugger", debugger,
				"Interface to the Windows debuggers", ScopedItemOptions.Constant));
			_host = new DbgPsHost(debugger, program);
			_runspace = RunspaceFactory.CreateRunspace(_host, initialSessionState);
		}

		public void Dispose()
		{
			_runspace.Close();
		}

		public static int Initialize(IDebugger debugger, IProgram program)
		{
			_theSession = new PSSession(debugger, program);
			return 0;
		}

		public static void Uninitialize()
		{
			_theSession.Dispose();
		}

		public static void InvokeCommand(string command)
		{
			_theSession.Invoke(command);
		}

		private void Invoke(string command)
		{
			if (!IsRunspaceOpen())
			{
				InitializePowerShell();
			}

			_pipelineDoneEvent.Reset();
			var pipeTask = Task.Factory.StartNew(() => Execute(command));
			pipeTask.ContinueWith(t => { _pipelineDoneEvent.Set(); }, TaskContinuationOptions.ExecuteSynchronously);
			_program.ProcessEvents(_pipelineDoneEvent);
			pipeTask.Wait();
		}

		private bool IsRunspaceOpen()
		{
			return _runspace.RunspaceStateInfo.State == RunspaceState.Opened;
		}

		private void InitializePowerShell()
		{
			_runspace.Open();
			LoadProfile();
		}

		private void LoadProfile()
		{
			_pipelineDoneEvent.Reset();
			var profileTask = Task.Factory.StartNew(InvokeProfileScripts);
			_program.ProcessEvents(_pipelineDoneEvent);
			profileTask.Wait();
		}

		private void InvokeProfileScripts()
		{
			var ps = PowerShell.Create();
			try
			{
				ps.Runspace = _runspace;
				var profileCommand = HostUtilities.GetProfileCommands("PSExt");
				foreach (var pc in profileCommand)
				{
					ps.Commands = pc;
					ps.Invoke();
				}
			}
			finally
			{
				_pipelineDoneEvent.Set();
				ps.Dispose();				
			}
		}

		/// <summary>
		///     A helper class that builds and executes a pipeline that writes
		///     to the default output path. Any exceptions that are thrown are
		///     just passed to the caller. Since all output goes to the default
		///     outter, this method does not return anything.
		/// </summary>
		/// <param name="cmd">The script to run.</param>
		/// <param name="input">
		///     Any input arguments to pass to the script.
		///     If null then nothing is passed in.
		/// </param>
		private void ExecuteHelper(string cmd, object input)
		{
			// Ignore empty command lines.
			if (String.IsNullOrEmpty(cmd))
			{
				return;
			}

			// Create the pipeline object and make it available to the
			// ctrl-C handle through the currentPowerShell instance
			// variable.
			lock (_instanceLock)
			{
				_currentPowerShell = PowerShell.Create();
			}

			// Add a script and command to the pipeline and then run the pipeline. Place 
			// the results in the currentPowerShell variable so that the pipeline can be 
			// stopped.
			try
			{
				_currentPowerShell.Runspace = _runspace;

				_currentPowerShell.AddScript(cmd);

				// Add the default outputter to the end of the pipe and then call the 
				// MergeMyResults method to merge the output and error streams from the 
				// pipeline. This will result in the output being written using the PSHost
				// and PSHostUserInterface classes instead of returning objects to the host
				// application.
				_currentPowerShell.AddCommand("out-default");
				_currentPowerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

				// If there is any input pass it in, otherwise just invoke the
				// the pipeline.
				if (input != null)
				{
					_currentPowerShell.Invoke(new[] {input});
				}
				else
				{
					_currentPowerShell.Invoke();
				}
			}
			finally
			{
				// Dispose the PowerShell object and set currentPowerShell to null. 
				// It is locked because currentPowerShell may be accessed by the 
				// ctrl-C handler.
				lock (_instanceLock)
				{
					_currentPowerShell.Dispose();
					_currentPowerShell = null;
				}
			}
		}

		/// <summary>
		///     To display an exception using the display formatter,
		///     run a second pipeline passing in the error record.
		///     The runtime will bind this to the $input variable,
		///     which is why $input is being piped to the Out-String
		///     cmdlet. The WriteErrorLine method is called to make sure
		///     the error gets displayed in the correct error color.
		/// </summary>
		/// <param name="e">The exception to display.</param>
		private void ReportException(Exception e)
		{
			if (e == null) return;
			var icer = e as IContainsErrorRecord;
			object error = icer != null ? icer.ErrorRecord : new ErrorRecord(e, "Host.ReportException", ErrorCategory.NotSpecified, null);

			lock (_instanceLock)
			{
				_currentPowerShell = PowerShell.Create();
			}

			_currentPowerShell.Runspace = _runspace;

			try
			{
				_currentPowerShell.AddScript("$input").AddCommand("out-string");

				// Do not merge errors, this function will swallow errors.
				var inputCollection = new PSDataCollection<object> {error};
				inputCollection.Complete();
				var result = _currentPowerShell.Invoke(inputCollection);

				if (result.Count > 0)
				{
					var str = result[0].BaseObject as string;
					if (!string.IsNullOrEmpty(str))
					{
						// Remove \r\n, which is added by the Out-String cmdlet.
						_host.UI.WriteErrorLine(str.Substring(0, str.Length - 2));
					}
				}
			}
			finally
			{
				// Dispose of the pipeline and set it to null, locking it  because 
				// currentPowerShell may be accessed by the ctrl-C handler.
				lock (_instanceLock)
				{
					_currentPowerShell.Dispose();
					_currentPowerShell = null;
				}
			}
		}

		/// <summary>
		///     Basic script execution routine. Any runtime exceptions are
		///     caught and passed back to the Windows PowerShell engine to
		///     display.
		/// </summary>
		/// <param name="cmd">Script to run.</param>
		private void Execute(string cmd)
		{
			try
			{
				// Run the command with no input.
				ExecuteHelper(cmd, null);
			}
			catch (RuntimeException rte)
			{
				ReportException(rte);
			}
		}

		private static IEnumerable<SessionStateCommandEntry> GetInitialCommands()
		{
			yield return new SessionStateCmdletEntry("Invoke-DbgCommand", typeof (InvokeDbgCommand), "");
			yield return new SessionStateCmdletEntry("Get-DbgBreakpoint", typeof (GetDbgBreakpointCommand), "");
			yield return new SessionStateCmdletEntry("Get-DbgModule", typeof (GetDebuggerModuleCommand), "");
			yield return new SessionStateAliasEntry("idc", "Invoke-DbgCommand");
		}
	}
}