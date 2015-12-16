#pragma once
#using <PSExtCmdlets.dll>

ref class Client;
ref class DebuggerBreakpoint;

ref class DebuggerProxy : public PSExt::IDebugger
{
	Client^ _client;	
public:
	DebuggerProxy();
	virtual System::String^ ExecuteCommand(System::String^ command);	
	virtual System::String^ ReadLine();
	virtual void Write(System::String^ output);
	virtual System::Collections::Generic::IList<PSExt::BreakpointData^>^ GetBreakpoints();
	virtual System::Collections::Generic::IList<PSExt::BreakpointData^>^ AddBreakpoints(PSExt::BreakpointData^ data);
	virtual System::Collections::Generic::IList<PSExt::ModuleData^>^ GetModules();
};

