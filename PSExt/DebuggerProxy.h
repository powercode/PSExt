#pragma once
#using <PSExtCmdlets.dll>

ref class Client;
ref class DebuggerBreakpoint;

ref class DebuggerProxy : public PSExt::IDebugger
{
	Client^ _client;
	DebuggerBreakpoint^ _breakpoints;
public:
	virtual System::String^ ExecuteCommand(System::String^ command);
	virtual System::Collections::Generic::List<PSExt::BreakpointData^>^ GetBreakpoints();
	virtual System::String^ ReadLine();
	virtual void Write(System::String^ output);
};

