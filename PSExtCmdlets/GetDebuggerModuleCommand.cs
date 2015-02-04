using System;
using System.Management.Automation;

namespace PSExt
{
	[Cmdlet(VerbsCommon.Get, "DbgModule")]
	[OutputType(typeof(SimpleDbgModule))]
	class GetDebuggerModuleCommand: DbgBaseCmdlet{	
		static readonly System.Text.RegularExpressions.Regex Pattern = new System.Text.RegularExpressions.Regex("(?<f>\\S+)\\s(?<t>\\S+)\\s+(?<m>\\S+)");

		protected override void EndProcessing() {
			var res = Debugger.ExecuteCommand("lm");
		
			foreach(var line in res.Split(new []{ '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)){
				var match = Pattern.Match(line);
				if (match.Success){
					var start = Convert.ToUInt64(match.Groups[1].Value.Remove(8,1), 16);
					var end = Convert.ToUInt64(match.Groups[2].Value.Remove(8, 1), 16);
					var name = match.Groups[3].Value;
					WriteObject(new SimpleDbgModule(start,end,name));
				}
			}
		}
	};
}