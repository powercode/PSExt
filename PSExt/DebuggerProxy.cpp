#include "DebuggerProxy.h"
#include "Client.h"
#include "Breakpoints.h"

using namespace System;
using namespace System::Collections::Generic;

DebuggerProxy::DebuggerProxy(){
	_client = gcnew Client();
	_breakpoints = gcnew DebuggerBreakpoint();
}

String^ DebuggerProxy::ExecuteCommand(String^ command)
{
	return _client->ExecuteCommand(command);
}


List<PSExt::BreakpointData^>^ DebuggerProxy::GetBreakpoints(){
	return DebuggerBreakpoint::GetBreakpoints();
}

String^ DebuggerProxy::ReadLine(){
	return _client->ReadLine();
}

void DebuggerProxy::Write(System::String^ output){
	_client->Write(output);
}

