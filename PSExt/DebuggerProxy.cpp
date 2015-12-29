#include "DebuggerProxy.h"
#include "Client.h"
#include "Breakpoints.h"
#include "DebuggerDispatcher.h"
#include "Module.h"
#include "Callstack.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace PSExt;

DebuggerProxy::DebuggerProxy(){
	_client = gcnew Client();
	
}

String^ DebuggerProxy::ExecuteCommand(String^ command)
{
	if (DebuggerDispatcher::Instance->DispatchRequired()){
		return (String^) DebuggerDispatcher::Instance->InvokeFunction(Client::typeid, _client, "ExecuteCommand", command);
		
	}	
	return _client->ExecuteCommand(command);
}

String^ DebuggerProxy::ReadLine(){
	if (DebuggerDispatcher::Instance->DispatchRequired()){
		return (String^) DebuggerDispatcher::Instance->InvokeFunction(Client::typeid, _client, "ReadLine");		
	}
	return _client->ReadLine();
}

void DebuggerProxy::Write(System::String^ output){
	if (DebuggerDispatcher::Instance->DispatchRequired()){
		DebuggerDispatcher::Instance->InvokeFunction(Client::typeid, _client, "Write", output);
		return;
	}
	_client->Write(output);
}

IList<BreakpointData^>^ DebuggerProxy::GetBreakpoints(){
	if (DebuggerDispatcher::Instance->DispatchRequired()){
		return (IList<BreakpointData^>^) DebuggerDispatcher::Instance->InvokeFunction(DebuggerProxy::typeid, this, "GetBreakpoints");
	}
	return DebuggerBreakpoint::GetBreakpoints();
}

IList<BreakpointData^>^ DebuggerProxy::AddBreakpoints(BreakpointData^ data){	
	if (DebuggerDispatcher::Instance->DispatchRequired()){
		return (IList<BreakpointData^>^) DebuggerDispatcher::Instance->InvokeFunction(DebuggerProxy::typeid, this, "AddBreakpoints", data);
	}

	return DebuggerBreakpoint::AddBreakpoints(data);
}

IList<ModuleData^>^ DebuggerProxy::GetModules() {
	if (DebuggerDispatcher::Instance->DispatchRequired()) {
		return (IList<ModuleData^>^) DebuggerDispatcher::Instance->InvokeFunction(DebuggerProxy::typeid, this, "GetModules");
	}
	return Modules::GetModules();
}

IList<StackFrame^>^ DebuggerProxy::GetCallstack() {
	if (DebuggerDispatcher::Instance->DispatchRequired()) {
		return (IList<StackFrame^>^) DebuggerDispatcher::Instance->InvokeFunction(DebuggerProxy::typeid, this,  "GetCallstack");
	}
	return Callstack::GetCallstacks();
}
