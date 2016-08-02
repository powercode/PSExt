using System.Management.Automation;

namespace PSExt.Commands
{
	[Cmdlet(VerbsCommon.Get, "Thread")]
	[OutputType(typeof(ThreadInfo))]
	[Alias("t")]
	public class GetThreadCommand : DbgBaseCmdlet
	{
		[Parameter]
		public SwitchParameter All { get; set; }
		protected override void ProcessRecord()
		{
			var threads = Debugger.GetCallstack(All);
			WriteObject(threads, true);
		}
	}
}