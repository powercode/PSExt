using System;

namespace PSExt
{
	[Flags]
	public enum DataBreakpointAccessTypes
	{
		None,
		Read,
		Write,
		ReadWrite = Read | Write,
		Execute,
		IO,
	};


}