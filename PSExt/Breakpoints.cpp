
#include "Breakpoints.h"
#include "DebuggerDispatcher.h"
#include <msclr/marshal_cppstd.h>
#include "NativeDebuggerBreakpoint.h"

using namespace System;
using namespace PSExt;
using namespace System::Collections::Generic;

List<BreakpointData^>^ DebuggerBreakpoint::GetBreakpoints(){

	if (DebuggerDispatcher::Instance->DispatchRequired()){
		auto res = (List<BreakpointData^>^)DebuggerDispatcher::Instance->InvokeFunction(DebuggerBreakpoint::typeid, "GetBreakpoints");
		return res;
	}
	auto bps = NativeDebuggerBreakpoint::GetBreakpoints();
	List<BreakpointData^>^ res = gcnew List<BreakpointData^>((int)bps.size());
	for (const auto& nbp : bps){
		auto& p = nbp.Parameters;
		auto k = (BreakType)p.BreakType;
		auto flags = (BreakpointFlags)p.Flags;
		auto dk = (DataBreakpointAccessTypes)p.DataAccessType;
			
		auto cmd = msclr::interop::marshal_as<String^>(nbp.Command);
		auto expr = msclr::interop::marshal_as<String^>(nbp.OffsetExpression);
		res->Add(gcnew BreakpointData(p.Offset, k, flags, dk, p.DataSize, p.ProcType, p.MatchThread,
			p.Id, p.PassCount, p.CurrentPassCount, cmd, expr));
	}
	return res;
}

