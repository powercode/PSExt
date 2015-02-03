#pragma once
#include "engextcpp.hpp"
#include <msclr\marshal.h>
#include "DebuggerDispatcher.h"

using namespace System::Management::Automation;


[CmdletAttribute(VerbsLifecycle::Invoke, "DbgCommand")]
[OutputType(System::String::typeid)]
ref class InvokeDebuggerCommand : PSCmdlet{	
public:	
	[Parameter(Mandatory = true, Position=1)]
	property System::String^ Command;
		
	void EndProcessing() override{

		auto res = DebuggerDispatcher::Instance->ExecuteCommand(Command);
		
		WriteObject(
			res->Split(gcnew array<wchar_t>(2){ '\r', '\n' }, System::StringSplitOptions::RemoveEmptyEntries),
			true);
	}	
};