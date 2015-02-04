#pragma once
#include "Breakpoints.h"
using namespace System::Management::Automation;

ref class GetDbgBreakpointCommand : public PSCmdlet{
public:
	void EndProcessing() override{
		auto bps = DebuggerBreakpoint::GetBreakpoints();
		WriteObject(bps, true);
	}
};