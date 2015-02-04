using System::String;
using System::GC;
using namespace System::Globalization;
using namespace System::Threading;
using namespace System::Management::Automation;

#include <msclr\marshal.h>
#include "DebuggerDispatcher.h"
#include "DebuggerProxy.h"
#include "PowerShellCommands.h"


ref class Program : PSExt::IProgram{
	DebuggerProxy^ _debugger = gcnew DebuggerProxy();	
	static Program^ TheProgram;
public:	
	Program(){

	}
	
	virtual void ProcessEvents(System::Threading::WaitHandle^ doneEvent) {
		DebuggerDispatcher::Instance->Start(doneEvent);
	}

	property bool ShouldExit{
		virtual bool get() {
			return false;
		}
		virtual void set(bool) {
		}
	}
	property int ExitCode{
		virtual int get() {
			return false;
		}
		virtual void set(int) {
		}
	}
	
	static int Initialize(){
		TheProgram = gcnew Program();
		return PSExt::PSSession::Initialize(TheProgram->_debugger, TheProgram);
	}

	static void InvokeCommand(String^ command) {
		PSExt::PSSession::InvokeCommand(command);		
	}

	
};


HRESULT InitializePowerShell(){	
	return Program::Initialize();
}


void InvokePowerShellCommand(PCSTR command){

	auto cmd = msclr::interop::marshal_as<String^>(command);
	PSExt::PSSession::InvokeCommand(cmd);
}

void UninitializePowerShell(){	
	PSExt::PSSession::Uninitialize();
	GC::WaitForFullGCComplete();
	GC::WaitForPendingFinalizers();
	
}



