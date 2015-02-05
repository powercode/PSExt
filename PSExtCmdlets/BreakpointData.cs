using System;

namespace PSExt
{
	public class BreakpointData
	{
		public uint Id { get; private set; }
		public ulong Offset { get; private set; }
		public BreakType BreakType { get; private set; }
		public BreakpointFlags Flags { get; private set; }
		public DataBreakpointAccessTypes DataBreakpointKind { get; private set; }
		public uint DataSize { get; private set; }
		public uint ProcType { get; private set; }
		//The engine thread ID of the thread that can trigger this breakpoint.
		public uint MatchThread { get; private set; }
		
		public uint PassCount { get; private set; }
		public uint CurrentPassCount { get; private set; }
		public string Command { get; private set; }
		public string Expression { get; private set; }

		public BreakpointData(UInt64 offset, BreakType breakType, BreakpointFlags flags,
			DataBreakpointAccessTypes dkind, UInt32 dataSize, UInt32 procType,
			UInt32 matchThread, UInt32 id, UInt32 passCount, UInt32 currentPasscount, String command, String expr)
		{
			Offset = offset;
			Flags = flags;
			BreakType = breakType;
			DataBreakpointKind = dkind;
			DataSize = dataSize;
			ProcType = procType;
			MatchThread = matchThread;
			Id = id;
			PassCount = passCount;
			CurrentPassCount = currentPasscount;
			Command = command;
			Expression = expr;			
		}		


	}
}