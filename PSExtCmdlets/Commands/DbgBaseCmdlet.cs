using System.Management.Automation;

namespace PSExt.Commands
{
	public abstract class DbgBaseCmdlet : PSCmdlet
	{
		private IDebugger _debugger;

		protected IDebugger Debugger
		{
			get
			{
				return _debugger ?? (_debugger = (IDebugger) SessionState.PSVariable.Get("Debugger").Value);
			}
		}

		protected DbgBaseCmdlet()
		{			
		}
	}
}