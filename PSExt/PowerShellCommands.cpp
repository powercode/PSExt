using System::String;
using System::GC;
using namespace System::Globalization;
using namespace System::Threading;
using namespace System::Management::Automation;

#include <msclr\marshal.h>
#include "Debugger.h"
#include "PowerShellCommands.h"


ref class Program : PSExt::IProgram{
	PSExt::IDebugger^ _debugger;	
	static Program^ TheProgram;
public:	
	Program(){
		PSExt::Native::Debugger^ debugger = gcnew PSExt::Native::Debugger();
		_debugger = gcnew PSExt::DebuggerProxy(debugger);
	}
	
	virtual void ProcessEvents(System::Threading::WaitHandle^ doneEvent) {
		PSExt::DebuggerDispatcher::Instance->Start(doneEvent);
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
			return 0;
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



