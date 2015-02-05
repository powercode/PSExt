#pragma once
#using <PSExtCmdlets.dll>
#include <dbgeng.h>

ref class DebuggerBreakpoint{
	
	static ULONG AddBreakpointOffset(ULONG64 offset, PSExt::BreakpointData^ data);
public:
	static System::Collections::Generic::IList<PSExt::BreakpointData^>^ GetBreakpoints();
	static System::Collections::Generic::IList<PSExt::BreakpointData^>^ AddBreakpoints(PSExt::BreakpointData^ data);
	
};