using System::String;
using namespace System::Globalization;
using namespace System::Management::Automation;


#include "engextcpp.hpp"
#include <msclr\marshal.h>
#include "PowerShellCommands.h"
#include "PowerShellHostUI.hpp"


using namespace msclr::interop;
ref class PowerShellHostUI;

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

	


	static String^ InvokeCommand(String^ command) {
		TheShell->AddScript(command);
		auto res = TheShell->Invoke();
		marshal_context context;
		
		for each(auto r in res){
			String^ str = r->ToString();
			const wchar_t* t = context.marshal_as<const wchar_t*>(str);
			g_ExtInstancePtr->Out(t);
		}
		return "Foobar";
	}
};

void InvokePowerShellCommand(PCSTR command){

	DbgPSHost::InvokeCommand(marshal_as<String^>(command));
}

HRESULT InitializeDbgPsHost(){
	auto host = gcnew DbgPSHost;
	auto runspace = Runspaces::RunspaceFactory::CreateRunspace(host);
	runspace->Open();

}
HRESULT UninitializeDbgPsHost(){
	

}



