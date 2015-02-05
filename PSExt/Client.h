#pragma once

#include <msclr/marshal.h>

ref class DebuggerDispatcher;

ref class Client
{
	msclr::interop::marshal_context _marshal_context;
public:
	System::String^ ExecuteCommand(System::String^ command);
	System::String^ ReadLine();
	void Write(System::String ^ output);
};

