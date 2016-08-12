using System.Linq;
using System.Management.Automation;
using DebugData;

namespace PSExt.Commands
{
	[Cmdlet(VerbsCommon.Get, "DbgModule")]
	[OutputType(typeof (ModuleData))]
	[Alias("lm")]
	public class GetDebuggerModuleCommand : DbgBaseCmdlet
	{
		[Parameter(Position = 1)]
		[SupportsWildcards]
		public string[] Include { get; set; }

		[Parameter(Position = 2)]
		[SupportsWildcards]
		public string[] Exclude { get; set; }

		protected override void EndProcessing()
		{
			var includePattern = Include?.Select(i => new WildcardPattern(i, WildcardOptions.IgnoreCase)).ToArray();
			var excludePattern = Exclude?.Select(i => new WildcardPattern(i, WildcardOptions.IgnoreCase)).ToArray();

			foreach (var mod in Debugger.GetModules())
			{
				if (excludePattern != null)
				{
					if (excludePattern.Any(ex => ex.IsMatch(mod.ModuleName)))
					{
						continue;
					}
				}
				if (includePattern == null || includePattern.Any(inc => inc.IsMatch(mod.ModuleName)))
				{
					WriteObject(mod);
				}
			}
		}
	}
}