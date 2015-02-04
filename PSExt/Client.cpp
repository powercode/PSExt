
#include "Client.h"
#include "engextcpp.hpp"
#include "DebuggerDispatcher.h"

using namespace System;

Client::Client(){
	_dispatcher = DebuggerDispatcher::Instance;
}


Object^ Client::DispatchFunctionCall(String^ method){
	return _dispatcher->InvokeFunction(Client::typeid, this, method);
}
Object^ Client::DispatchFunctionCall(String^ method, Object^ arg1){
	return _dispatcher->InvokeFunction(Client::typeid, this, method, arg1);
}

String^ Client::ExecuteCommand(String^ command){
	if (_dispatcher->DispatchRequired()){
		return (String^)_dispatcher->InvokeFunction(Client::typeid, this, "ExecuteCommand", command);
	}

	auto cmd = _marshal_context.marshal_as<PCSTR>(command);
	ExtCaptureOutputW outputCapture;
	outputCapture.Execute(cmd);
	auto res = outputCapture.GetTextNonNull();
	auto mres = msclr::interop::marshal_as<String^>(res);
	return mres;
}


String^ Client::ReadLine(){
	if (_dispatcher->DispatchRequired()){
		return (String^)DispatchFunctionCall("ReadLine");
	}

	wchar_t buf[256];
	ULONG inputSize = 0;
	g_ExtInstancePtr->m_Control4->InputWide(buf, 256, &inputSize);
	return msclr::interop::marshal_as<String^>(buf);
}

void Client::Write(String ^ output) {
	if (_dispatcher->DispatchRequired()){
		DispatchFunctionCall("Write", output);
		return;
	}
	auto str = _marshal_context.marshal_as<PCWSTR>(output);
	g_ExtInstancePtr->Out(str);
}
