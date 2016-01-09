using System.Management.Automation;

namespace PSExt.Commands
{
	[Cmdlet(VerbsCommon.Get, "Callstack")]
	[OutputType(typeof (Callstack))]
	[Alias("k")]
	public class GetCallstackCommand : DbgBaseCmdlet
	{
		protected override void ProcessRecord()
		{
			WriteObject(Debugger.GetCallstack(), true);
		}
	}
}