using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PSExt.Host;

namespace PSExt.Extension
{
	class ConsolePSSession : PSSession, IInvokeInteractive
	{
		
		private readonly ConsoleHost _consoleHost;		
		private readonly ConsoleReadLine _consoleReadLine;
		public ConsolePSSession(IDebugger debugger, ConsoleHost host, IMethodCallDispatch methodCallDispatcher) : base(debugger, host, methodCallDispatcher)
		{
			host.Program = this;
			_consoleHost = host;			
			_consoleReadLine = new ConsoleReadLine();
		}

		public Runspace Runspace { get { return _runspace; } set { _runspace = value; } }

		public override bool SupportsInteractive => true;
		public bool ShouldExit { get; set; }
		public int ExitCode { get; set; }
		
		protected override void InitialisePowerShellImpl()
		{
			var ps = PowerShell.Create();
			ps.Runspace = _runspace;
			var readlineFunction = @"
function PSConsoleHostReadline
{
	[PSExt.Host.ConsoleReadline]::Read()
}
Import-Module PSReadline -EA:0
";
			ps.AddScript(readlineFunction);
			ps.Invoke();
		}		

		string GetScriptResultOrDefault(string command, string defaultValue)
		{
			var res = InvokeScript<string>(command);
			if (res == null || res.Count == 0)
			{
				return defaultValue;
			}
			return res[0];
		}

		string ReadLine()
		{			
			return GetScriptResultOrDefault("PSConsoleHostReadline", null);
		}

		string GetPrompt()
		{
			return GetScriptResultOrDefault("prompt", "PSDBG>");			
		}
		
		void IInvokeInteractive.Run()
		{
			InitialisePowerShellImpl();
			ShouldExit = false;			
			// Set up the control-C handler.
			var treatAsInputOld = Console.TreatControlCAsInput;
			Console.CancelKeyPress += HandleControlC;			
			Console.TreatControlCAsInput = false;
			try {
				// Read commands and run them until the ShouldExit flag is set by
				// the user calling "exit".
				var invocationSettings = new PSInvocationSettings{AddToHistory = true};
				while (!ShouldExit)
				{

					var prompt = GetPrompt();				
					_consoleHost.UI.Write(_consoleHost.UI.RawUI.ForegroundColor, _consoleHost.UI.RawUI.BackgroundColor, prompt);
					var cmd = ReadLine() ?? _consoleReadLine.Read();					
					InvokeScript(cmd, invocationSettings);				
				}
			}
			finally {
				Console.CancelKeyPress -= HandleControlC;
				Console.TreatControlCAsInput = treatAsInputOld;
			}

			// Exit with the desired exit code that was set by the exit command.
			// The exit code is set in the host by the MyHost.SetShouldExit() method.						
		}

		private void HandleControlC(object sender, ConsoleCancelEventArgs e)
		{
			try
			{
				lock (_instanceLock)
				{
					if (_currentPowerShell != null && _currentPowerShell.InvocationStateInfo.State == PSInvocationState.Running)
					{
						_currentPowerShell.Stop();
					}
				}

				e.Cancel = true;
			}
			catch (Exception exception)
			{
				_consoleHost.UI.WriteErrorLine(exception.ToString());
			}
			ShouldExit = true;
			ExitCode = 1;
		}

		public override string ShellId => "ConsolePSExt";
	}
}