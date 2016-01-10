#pragma once
#using <PSExtCmdlets.dll>
#include <msclr/marshal.h>

namespace PSExt { namespace Native {

ref class Debugger : public PSExt::IDebugger
{
	msclr::interop::marshal_context _marshal_context;
public:	
	virtual System::String^ ExecuteCommand(System::String^ command);	
	virtual System::String^ ReadLine();
	virtual void Write(System::String^ output);
	virtual System::Collections::Generic::IList<PSExt::BreakpointData^>^ GetBreakpoints();
	virtual System::Collections::Generic::IList<PSExt::BreakpointData^>^ AddBreakpoints(PSExt::BreakpointData^ data);
	virtual System::Collections::Generic::IList<PSExt::ModuleData^>^ GetModules();
	virtual PSExt::DebugThread^ GetCallstack();
};

}}