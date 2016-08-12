using System;

namespace DebugData
{
	[Flags]
	public enum DataBreakpointAccessTypes
	{
		None,
		Read,
		Write,
		ReadWrite = Read | Write,
		Execute,
		IO
	}
}