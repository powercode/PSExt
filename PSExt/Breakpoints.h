#pragma once

ref class DebuggerBreakpoint{
public:
	static System::Collections::Generic::List<PSExt::BreakpointData^>^ GetBreakpoints();
};