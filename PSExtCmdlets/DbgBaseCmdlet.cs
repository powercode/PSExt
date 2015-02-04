using System.Management.Automation;

namespace PSExt
{
	public abstract class DbgBaseCmdlet : PSCmdlet
	{
		protected IDebugger Debugger { get; private set; }

		protected DbgBaseCmdlet()
		{
			Debugger = (IDebugger) SessionState.PSVariable.Get("Debugger").Value;
		}
	}
}