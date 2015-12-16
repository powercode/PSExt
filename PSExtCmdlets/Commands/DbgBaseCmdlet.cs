using System.Management.Automation;

namespace PSExt.Commands
{
	public abstract class DbgBaseCmdlet : PSCmdlet
	{
		private IDebugger _debugger;

		protected IDebugger Debugger => _debugger ?? (_debugger = (IDebugger) SessionState.PSVariable.Get("Debugger").Value);

		protected DbgBaseCmdlet()
		{			
		}
	}
}