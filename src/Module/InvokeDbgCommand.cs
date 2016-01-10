using System;
using System.Management.Automation;

namespace PSExt.Commands
{
	[Cmdlet(VerbsLifecycle.Invoke, "DbgCommand")]
	[OutputType(typeof (string))]
	[Alias("idc")]
	public class InvokeDbgCommand : DbgBaseCmdlet
	{
		[Parameter(Mandatory = true, Position = 1)]
		public string Command { get; set; }

		protected override void EndProcessing()
		{
			var res = Debugger.ExecuteCommand(Command);
			var splitRes = res.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
			WriteObject(splitRes, true);
		}
	}
}