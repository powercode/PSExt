using System.Management.Automation.Host;

namespace PSExt.Extension
{
	class DbgEnginePSSession : PSSession
	{
		
		public DbgEnginePSSession(IDebugger debugger, PSHost host, IMethodCallDispatch methodCallDispatcher) : base(debugger, host, methodCallDispatcher)
		{
		
		}
		public override bool SupportsInteractive => false;		

		protected override void InitialisePowerShellImpl()
		{
		}

		public override string ShellId => "DbgEngPSExt";
	}
}