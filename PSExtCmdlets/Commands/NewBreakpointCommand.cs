using System.Management.Automation;

namespace PSExt.Commands
{
	[Cmdlet(VerbsCommon.New, "DbgBreakpoint", DefaultParameterSetName = "CodeExpr")]
	[OutputType(typeof (BreakpointData))]
	public class NewBreakpointCommand : DbgBaseCmdlet
	{
		public NewBreakpointCommand()
		{
			Id = uint.MaxValue;
			Flags = BreakpointFlags.Enabled;
		}

		[Parameter(Mandatory = true, ParameterSetName = "CodeExpr", ValueFromPipelineByPropertyName = true, Position = 1)]
		[Parameter(Mandatory = true, ParameterSetName = "DataExpr", ValueFromPipelineByPropertyName = true, Position = 1)]
		public string OffsetExpression { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = "CodeOffset", ValueFromPipelineByPropertyName = true)]
		[Parameter(Mandatory = true, ParameterSetName = "DataOffset", ValueFromPipelineByPropertyName = true)]
		public ulong Offset { get; set; }

		[Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "DataExpr")]
		[Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "DataOffset")]
		public uint DataSize { get; set; }

		[Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "DataExpr")]
		[Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "DataOffset")]
		public DataBreakpointAccessTypes DataAccess { get; set; }

		[Parameter(ValueFromPipelineByPropertyName = true)]
		public BreakpointFlags Flags { get; set; }


		[Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
		public string Command { get; set; }

		[Parameter(ValueFromPipelineByPropertyName = true)]
		public uint MatchThread { get; set; }


		[Parameter(ValueFromPipelineByPropertyName = true)]
		public uint PassCount { get; set; }

		[Parameter(ValueFromPipelineByPropertyName = true)]
		public uint Id { get; set; }


		protected override void ProcessRecord()
		{
			var bt = BreakType.Code;
			switch (ParameterSetName)
			{
				case "DataOffset":
				case "DataExpr":
					bt = BreakType.Data;
					break;
			}

			var bd = new BreakpointData(Offset, bt, Flags, DataAccess, DataSize, 0, MatchThread, Id, PassCount, 0,
				Command, OffsetExpression);
			var res = Debugger.AddBreakpoints(bd);
			WriteObject(res, true);
		}
	}
}