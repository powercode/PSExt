
#include "engextcpp.hpp"
#include <msclr\marshal.h>

using namespace System::Management::Automation;


[CmdletAttribute(VerbsLifecycle::Invoke, "DebuggerCommand")]
ref class InvokeDebuggerCommand : PSCmdlet{
	msclr::interop::marshal_context _context;	
public:
	
	[Parameter(Mandatory = true, Position=1)]
	property System::String^ Command;
		
	void EndProcessing() override{
		auto cmd = _context.marshal_as<PCSTR>(Command);
		ExtCaptureOutputW outputCapture;
		outputCapture.Execute(cmd);
		auto res = outputCapture.GetTextNonNull();
		auto mres = msclr::interop::marshal_as<String^>(res);
		WriteObject(mres);
	}	
};