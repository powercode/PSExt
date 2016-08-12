using System.Management.Automation;
using DebugData;

namespace PSExt.Commands
{
	[Cmdlet(VerbsCommon.Get, "DbgBreakpoint")]
	[OutputType(typeof (BreakpointData))]
	[Alias("bl")]
	public class GetDbgBreakpointCommand : DbgBaseCmdlet
	{
		protected override void EndProcessing()
		{
			var breakpoints = Debugger.GetBreakpoints();
			WriteObject(breakpoints, true);
		}
	}
}