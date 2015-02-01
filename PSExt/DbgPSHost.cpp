using System::String;
using namespace System::Globalization;
using namespace System::Management::Automation;
#include <msclr\marshal.h>

#include "engextcpp.hpp"

#include "PowerShellCommands.h"
#include "PowerShellHostUI.hpp"
#include "InvokeDebuggerCommand.h"


ref class DbgPSHost : Host::PSHost  {
	System::Guid _myId;
	CultureInfo^ _originalCulture;
	CultureInfo^ _originalUICulture;
	PowerShellHostUI^ _ui;
public:
	DbgPSHost(){
		_myId == System::Guid::NewGuid();
		_originalCulture = CultureInfo::CurrentCulture;
		_originalUICulture = CultureInfo::CurrentUICulture;
		_ui = gcnew PowerShellHostUI;
	}

	property CultureInfo^ CurrentCulture{
		CultureInfo^ get() override {
			return _originalCulture;
		}
	}
	property CultureInfo^ CurrentUICulture{
		CultureInfo^ get() override {
			return _originalUICulture;
		}
	}

	property System::Guid InstanceId {
		System::Guid get() override {
			return _myId;
		}
	}

	property System::Version^ Version {
		System::Version^ get() override {
			return gcnew System::Version(1,0,0,0);
		}
	}

	property System::String^ Name{
		System::String^ get() override {
			return "DbgExtHost";
		}
	}

	property Host::PSHostUserInterface^ UI{
		Host::PSHostUserInterface^ get() override {
			return _ui;
		}
	}

	static PowerShell^ _powerShell;
	static property PowerShell^ TheShell{
		PowerShell^ get(){
			if (_powerShell == nullptr){
				_powerShell = PowerShell::Create();
			}
			return _powerShell;
		}
	}	

	void NotifyBeginApplication() override {}
	void NotifyEndApplication() override {}

	void SetShouldExit(int ) override {		
	}

	void EnterNestedPrompt() override {
		throw gcnew System::NotImplementedException("The method is not implemented.");
	}
	void ExitNestedPrompt() override {
		throw gcnew System::NotImplementedException("The method is not implemented.");
	}

	
	static int Initialize(){
		auto host = gcnew DbgPSHost;
		auto iss = Runspaces::InitialSessionState::Create();
		
		iss->Commands->Add(gcnew Runspaces::SessionStateCmdletEntry("Invoke-DebuggerCommand", InvokeDebuggerCommand::typeid, nullptr));
		iss->Commands->Add(gcnew Runspaces::SessionStateAliasEntry("idc", "Invoke-DebuggerCommand", ""));

		_runspace = Runspaces::RunspaceFactory::CreateRunspace(host, iss);				
		return S_OK;
	}

	static void Uninitialize(){
		_runspace->~Runspace();
	}

	static void InvokeCommand(String^ command) {	
		if (_runspace->RunspaceStateInfo->State == Runspaces::RunspaceState::BeforeOpen){
			_runspace->Open();
		}
		PowerShell^ ps = PowerShell::Create();
		try{
			ps->Runspace = _runspace;
			ps->AddScript(command);
			ps->Invoke();
		}
		finally{			
			ps->~PowerShell();			
		}		
	}
private:
	static Runspaces::Runspace^ _runspace;
};

void InvokePowerShellCommand(PCSTR command){
	auto cmd = msclr::interop::marshal_as<String^>(command);
	DbgPSHost::InvokeCommand(cmd);
}

HRESULT InitializeDbgPsHost(){
	return DbgPSHost::Initialize();
}
void UninitializeDbgPsHost(){	
	DbgPSHost::Uninitialize();
}



