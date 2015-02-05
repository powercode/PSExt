#include "DebuggerProxy.h"
#include "Client.h"
#include "Breakpoints.h"
#include "DebuggerDispatcher.h"

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
		return (IList<BreakpointData^>^) DebuggerDispatcher::Instance->InvokeFunction(DebuggerBreakpoint::typeid, "GetBreakpoints");
	}
	return DebuggerBreakpoint::GetBreakpoints();
}

IList<BreakpointData^>^ DebuggerProxy::AddBreakpoints(BreakpointData^ data){	
	if (DebuggerDispatcher::Instance->DispatchRequired()){
		return (IList<BreakpointData^>^) DebuggerDispatcher::Instance->InvokeFunction(DebuggerBreakpoint::typeid, "AddBreakpoints", data);
	}

	return DebuggerBreakpoint::AddBreakpoints(data);
}
