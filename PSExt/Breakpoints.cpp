
#include "Breakpoints.h"
#include "DebuggerDispatcher.h"
#include <msclr/marshal_cppstd.h>
#include "NativeDebuggerBreakpoint.h"
#include "Symbols.h"

using namespace std;

using namespace System;
using namespace PSExt;
using namespace System::Collections::Generic;
using namespace msclr::interop;

BreakpointData^ ToBreakpoint(const NativeBreakpointData& data){
	auto& p = data.Parameters;
	auto k = (BreakType)p.BreakType;
	auto flags = (BreakpointFlags)p.Flags;
	auto dk = (DataBreakpointAccessTypes)p.DataAccessType;

	auto cmd = marshal_as<String^>(data.Command);
	auto expr = marshal_as<String^>(data.OffsetExpression);
	return gcnew BreakpointData(p.Offset, k, flags, dk, p.DataSize, p.ProcType, p.MatchThread,
		p.Id, p.PassCount, p.CurrentPassCount, cmd, expr);
}

IList<BreakpointData^>^ DebuggerBreakpoint::GetBreakpoints(){

	if (DebuggerDispatcher::Instance->DispatchRequired()){
		auto res = (List<BreakpointData^>^)DebuggerDispatcher::Instance->InvokeFunction(DebuggerBreakpoint::typeid, "GetBreakpoints");
		return res;
	}
	auto bps = NativeDebuggerBreakpoint::GetBreakpoints();
	List<BreakpointData^>^ res = gcnew List<BreakpointData^>((int)bps.size());
	for (const auto& nbp : bps){		
		res->Add(ToBreakpoint(nbp));
	}
	return res;
}
ULONG DebuggerBreakpoint::AddBreakpointOffset(ULONG64 offset, BreakpointData^ data){
	PDEBUG_BREAKPOINT2 pBp;
	auto res = g_Ext->m_Control4->AddBreakpoint2(
		static_cast<ULONG>(data->BreakType),
		data->Id,
		&pBp);
	if (FAILED(res)){
		throw std::runtime_error("failed to set breakpoint");
	}
	pBp->SetOffset(offset);
	{
		marshal_context context;		
		if (data->Command != nullptr){
			pBp->SetCommandWide(context.marshal_as<PCWSTR>(data->Command));
		}
	}	
	if (data->PassCount != 0){
		pBp->SetPassCount(data->PassCount);
	}
	if (data->MatchThread != 0){
		pBp->SetMatchThreadId(data->MatchThread);
	}
	if (data->MatchThread != 0){
		pBp->SetMatchThreadId(data->MatchThread);
	}
	if (data->BreakType == BreakType::Data){
		pBp->SetDataParameters(data->DataSize, static_cast<ULONG>(data->DataBreakpointKind));
	}
	if (data->Flags != BreakpointFlags::None){
		pBp->AddFlags(static_cast<ULONG>(data->Flags));
	}
	ULONG newId;
	if (FAILED(pBp->GetId(&newId))){
		throw std::runtime_error("failed to get breakpoint id for new breakpoint");
	}
	return newId;
}
IList<BreakpointData^>^ DebuggerBreakpoint::AddBreakpoints(BreakpointData^ data){
	auto retVal = gcnew List<BreakpointData^>();
	auto ids = gcnew List<UInt32>();
	ULONG bpid;
	if (data->Expression != nullptr){
		marshal_context context;
		auto pattern = context.marshal_as<PCWSTR>(data->Expression);
		auto matches = Symbols::GetMatchingSymbols(pattern); 
		if (matches.size() == 0){

		}
		else{
			for (auto& match : matches){
				bpid = AddBreakpointOffset(match.Offset, data);
				ids->Add(bpid);
			}
		}
	}
	else {
		bpid = AddBreakpointOffset(data->Offset, data);
		ids->Add(bpid);

	}
	auto bps = NativeDebuggerBreakpoint::GetBreakpoints();
	for (auto& bp : bps){		
		if (ids->Contains(bp.Parameters.Id)){
			retVal->Add(ToBreakpoint(bp));
		}
	}

	return retVal;
}
