using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PSExt.Host;

namespace PSExt.Extension
{
	interface IInvokeInteractive
	{
		void Run();
	}

	public abstract class PSSession : IDisposable
	{		
		protected readonly PSHost _host;
		protected readonly object _instanceLock = new object();
		protected Runspace _runspace;
		protected PowerShell _currentPowerShell;
		private readonly IMethodCallDispatch _methodCallDispatcher;
		private readonly AutoResetEvent _pipelineDoneEvent = new AutoResetEvent(false);


		protected PSSession(IDebugger debugger, PSHost host, IMethodCallDispatch methodCallDispatcher)
		{
			_methodCallDispatcher = methodCallDispatcher;
			var initialSessionState = InitialSessionState.CreateDefault();
			initialSessionState.Variables.Add(new SessionStateVariableEntry("Debugger", debugger,
				"Interface to the Windows debuggers", ScopedItemOptions.Constant));
			initialSessionState.Variables.Add(new SessionStateVariableEntry("ShellID", "PSExt", "", ScopedItemOptions.Constant));
			
			// Add the executing assembly as a PowerShell module since we have cmdlets here we want to execute.
			// This should be refactored into separate assemblies, since we could use the debugger parts outside
			// of an extension
			var location = Assembly.GetExecutingAssembly().Location;
			initialSessionState.ImportPSModule(new[] { location });

			// Extend types for easier display			
			var psextDir = Path.GetDirectoryName(location);
			var typeFile = Path.Combine(psextDir, "PSExt.Types.ps1xml");
			initialSessionState.Types.Add(new SessionStateTypeEntry(typeFile));

			// Pretty formatting of debugger output
			var formatFile = Path.Combine(psextDir, "PSExt.Format.ps1xml");
			initialSessionState.Formats.Add(new SessionStateFormatEntry(formatFile));
			_host = host;
			_runspace = RunspaceFactory.CreateRunspace(_host, initialSessionState);
		}

		public abstract bool SupportsInteractive { get; }		

		public void Dispose()
		{
			_runspace.Close();
		}			

		public void Invoke(string command)
		{
			if (!IsRunspaceOpen())
			{
				InitializePowerShell();
			}
			var settings = new PSInvocationSettings() {AddToHistory = true};
			InvokeScript(command, settings);								
		}

		public bool RunInteractive()
		{
			if (!IsRunspaceOpen())
			{
				InitializePowerShell();
			}
			var interactive = this as IInvokeInteractive;
			if (interactive == null) return false;
			interactive.Run();
			return true;
		}

		protected void InvokeScript(string script, PSInvocationSettings invocationSettings)
		{
			_pipelineDoneEvent.Reset();
			var pipeTask = Task.Factory.StartNew(() => Execute(script, invocationSettings));
			pipeTask.ContinueWith(t => { _pipelineDoneEvent.Set(); }, TaskContinuationOptions.ExecuteSynchronously);
			_methodCallDispatcher.DispatchMethodCalls(_pipelineDoneEvent);
			pipeTask.Wait();
		}

		protected Collection<T> InvokeScript<T>(string script)
		{
			_pipelineDoneEvent.Reset();
			var pipeTask = Task<Collection<T>>.Factory.StartNew(() => Execute<T>(script));
			pipeTask.ContinueWith(t => { _pipelineDoneEvent.Set(); }, TaskContinuationOptions.ExecuteSynchronously);
			_methodCallDispatcher.DispatchMethodCalls(_pipelineDoneEvent);
			pipeTask.Wait();
			return pipeTask.Result;
		}

		protected void InvokeCommand(PSCommand command)
		{
			_pipelineDoneEvent.Reset();
			var pipeTask = Task.Factory.StartNew(() => Execute(command));
			pipeTask.ContinueWith(t => { _pipelineDoneEvent.Set(); }, TaskContinuationOptions.ExecuteSynchronously);
			_methodCallDispatcher.DispatchMethodCalls(_pipelineDoneEvent);
			pipeTask.Wait();
		}

		public void InvokeInteractive()
		{
			
			if (!SupportsInteractive)
			{
				throw new InvalidOperationException();
			}
			if (!IsRunspaceOpen())
			{
				InitializePowerShell();
			}

						

		}

		private bool IsRunspaceOpen()
		{
			return _runspace.RunspaceStateInfo.State == RunspaceState.Opened;
		}

		protected void InitializePowerShell()
		{
			try
			{
				_runspace.Open();
				InitialisePowerShellImpl();
				LoadProfile();
			}
			catch (Exception c)
			{
				Debug.WriteLine(c.ToString());
			}
			var ps = PowerShell.Create();
			try
			{				
				ps.Runspace = _runspace;
				
			}
			finally
			{				
				ps.Dispose();
			}			
		}

		protected abstract void InitialisePowerShellImpl();


		private void LoadProfile()
		{
			var profileCommand = HostUtilities.GetProfileCommands(ShellId);
			foreach (var pc in profileCommand)
			{
				InvokeCommand(pc);				
			}			
		}

		public abstract string ShellId { get; }

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
		/// <param name="invocationSettings"></param>
		private void ExecuteHelper(string cmd, object input, PSInvocationSettings invocationSettings = null)
		{
			// Ignore empty command lines.
			if (string.IsNullOrEmpty(cmd))
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
				_currentPowerShell.Invoke(input != null ? new[] {input} : new PSObject[] {}, invocationSettings);
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
		private Collection<T> ExecuteHelper<T>(string cmd, object input)
		{
			// Ignore empty command lines.
			if (string.IsNullOrEmpty(cmd))
			{
				return new Collection<T>();
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
				
				// If there is any input pass it in, otherwise just invoke the
				// the pipeline.
				if (input != null)
				{
					return _currentPowerShell.Invoke<T>(new[] { input });
				}
				else
				{
					return _currentPowerShell.Invoke<T>();
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
		///     A helper class that builds and executes a pipeline that writes
		///     to the default output path. Any exceptions that are thrown are
		///     just passed to the caller. Since all output goes to the default
		///     outter, this method does not return anything.
		/// </summary>
		/// <param name="command">The PSCommand to run.</param>
		/// <param name="input">
		///     Any input arguments to pass to the script.
		///     If null then nothing is passed in.
		/// </param>
		private void ExecuteHelper(PSCommand command, object input)
		{
			// Ignore empty command lines.
			if (command == null )
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

				_currentPowerShell.Commands = command;

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
					_currentPowerShell.Invoke(new[] { input });
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
			object error = icer != null
				? icer.ErrorRecord
				: new ErrorRecord(e, "Host.ReportException", ErrorCategory.NotSpecified, null);

			lock (_instanceLock)
			{
				_currentPowerShell = PowerShell.Create();
			}

			_currentPowerShell.Runspace = _runspace;

			try
			{				
				_currentPowerShell.AddScript("$input").AddCommand("out-string");

				// Do not merge errors, this function will swallow errors.
				var inputCollection = new PSDataCollection<object> { error };
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
		/// <param name="cmd">Command to run.</param>
		protected void Execute(PSCommand cmd)
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

		/// <summary>
		///     Basic script execution routine. Any runtime exceptions are
		///     caught and passed back to the Windows PowerShell engine to
		///     display.
		/// </summary>
		/// <param name="cmd">Script to run.</param>
		/// <param name="invocationSettings"></param>
		protected void Execute(string cmd, PSInvocationSettings invocationSettings = null)
		{
			try
			{
				// Run the command with no input.
				ExecuteHelper(cmd, null, invocationSettings);
			}
			catch (RuntimeException rte)
			{
				ReportException(rte);
			}
		}

		/// <summary>
		///     Basic script execution routine. Any runtime exceptions are
		///     caught and passed back to the Windows PowerShell engine to
		///     display.
		/// </summary>
		/// <param name="cmd">Script to run.</param>
		protected Collection<T> Execute<T>(string cmd)
		{
			try
			{
				// Run the command with no input.
				return ExecuteHelper<T>(cmd, null);
			}
			catch (RuntimeException rte)
			{
				ReportException(rte);
				return null;				
			}
		}


	}
}