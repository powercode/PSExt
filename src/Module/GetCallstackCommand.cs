using System.Linq;
using System.Management.Automation;

namespace PSExt.Commands
{
	[Cmdlet(VerbsCommon.Get, "Thread")]
	[OutputType(typeof (StackFrame))]
	[Alias("k")]
	public class GetCallstackCommand : DbgBaseCmdlet
	{
		[Parameter]
		public SwitchParameter All { get; set; }
		protected override void ProcessRecord()
		{
			WriteObject(Debugger.GetCallstack(All).SelectMany(c=>c.Frames), true);
		}
	}
}