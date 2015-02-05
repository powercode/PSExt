using System;
using System.Management.Automation;

namespace PSExt.Commands {
	[Cmdlet(VerbsCommon.New, "DbgBreakpoint", DefaultParameterSetName = "CodeExpr")]
	[OutputType(typeof(BreakpointData))]
	public class NewBreakpointCommand :DbgBaseCmdlet{
		public NewBreakpointCommand()
		{
			Id = UInt32.MaxValue;
			Flags = BreakpointFlags.Enabled;
		}

		[Parameter(Mandatory = true, ParameterSetName = "CodeExpr", ValueFromPipelineByPropertyName = true, Position = 1)]
		[Parameter(Mandatory = true, ParameterSetName = "DataExpr", ValueFromPipelineByPropertyName = true, Position = 1)]
		public String OffsetExpression { get; set; }

		[Parameter(Mandatory = true, ParameterSetName = "CodeOffset", ValueFromPipelineByPropertyName = true)]
		[Parameter(Mandatory = true, ParameterSetName = "DataOffset", ValueFromPipelineByPropertyName = true)]
		public UInt64 Offset { get; set; }
		
		[Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "DataExpr")]
		[Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "DataOffset")]
		public UInt32 DataSize { get; set; }

		[Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "DataExpr")]
		[Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "DataOffset")]
		public DataBreakpointAccessTypes DataAccess { get; set; }

		[Parameter(ValueFromPipelineByPropertyName = true)]
		public BreakpointFlags Flags { get; set; }


		[Parameter(ValueFromPipelineByPropertyName = true, Position = 2)]
		public String Command { get; set; }

		[Parameter(ValueFromPipelineByPropertyName = true)]
		public UInt32 MatchThread { get; set; }


		[Parameter(ValueFromPipelineByPropertyName = true)]
		public UInt32 PassCount { get; set; }

		[Parameter(ValueFromPipelineByPropertyName = true)]
		public UInt32 Id { get; set; }


		protected override void ProcessRecord()
		{
			BreakType bt = BreakType.Code;
			switch (ParameterSetName)
			{
				case "DataOffset":
				case "DataExpr":
					bt = BreakType.Data;
					break;
				
			}

			BreakpointData bd = new BreakpointData(Offset, bt, Flags, DataAccess, DataSize, 0, MatchThread, Id, PassCount, 0,
				Command, OffsetExpression);
			var res = Debugger.AddBreakpoints(bd);
			WriteObject(res, enumerateCollection:true);
		}
	}
}
