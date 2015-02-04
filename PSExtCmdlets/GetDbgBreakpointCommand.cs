using System.Management.Automation;

namespace PSExt
{
	[Cmdlet(VerbsCommon.Get, "DbgBreakpoint")]
	[OutputType(typeof(BreakpointData))]
	public class GetDbgBreakpointCommand : DbgBaseCmdlet
	{
		protected override void EndProcessing()
		{
			var breakpoints = Debugger.GetBreakpoints();
			WriteObject(breakpoints, enumerateCollection:true);
		}
	}
}