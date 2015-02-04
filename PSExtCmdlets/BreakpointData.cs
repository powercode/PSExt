using System;

namespace PSExt
{
	public class BreakpointData
	{
		private UInt64 _offset;

		private BreakType _breakType;
		private BreakpointFlags _flags;

		private DataBreakpointAccessTypes _dataBreakpointKind;
		private UInt32 _dataSize;
		private UInt32 _procType;
		//The engine thread ID of the thread that can trigger this breakpoint.
		private UInt32 _matchThread;
		private UInt32 _id;
		private UInt32 _passcount;
		private UInt32 _currentPasscount;
		private String _command;
		private String _expression;

		public BreakpointData(UInt64 offset, BreakType breakType, BreakpointFlags flags,
			DataBreakpointAccessTypes dkind, UInt32 dataSize, UInt32 procType,
			UInt32 matchThread, UInt32 id, UInt32 passCount, UInt32 currentPasscount, String command, String expr)
		{
			_offset = offset;
			_flags = flags;
			_breakType = breakType;
			_dataBreakpointKind = dkind;
			_dataSize = dataSize;
			_procType = procType;
			_matchThread = matchThread;
			_id = id;
			_passcount = passCount;
			_currentPasscount = currentPasscount;
			_command = command;
			_expression = expr;
			{
			}
		}
	}
}