#pragma once

#include <msclr/marshal.h>

ref class DebuggerDispatcher;

ref class Client
{
	msclr::interop::marshal_context _marshal_context;
	DebuggerDispatcher^ _dispatcher;
	System::Object^ DispatchFunctionCall(System::String^ method);
	System::Object^ DispatchFunctionCall(System::String^ method, System::Object^ arg1);
public:
	Client();
	System::String^ ExecuteCommand(System::String^ command);
	System::String^ ReadLine();
	void Write(System::String ^ output);
};

