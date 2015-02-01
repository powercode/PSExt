#include "PowerShellHostRawUI.hpp"
#include <msclr\marshal.h>


ref class PowerShellHostUI : System::Management::Automation::Host::PSHostUserInterface{
	msclr::interop::marshal_context _marshal_context;

	PowerShellHostRawUI^ _rawUi = gcnew PowerShellHostRawUI;
public:
	property System::Management::Automation::Host::PSHostRawUserInterface^ RawUI{
		System::Management::Automation::Host::PSHostRawUserInterface^ get() override{
			return _rawUi;
		}
	}

	System::String ^ReadLine(void)override {
		wchar_t buf[256];
		ULONG inputSize = 0;
		g_ExtInstancePtr->m_Control4->InputWide(buf, 256, &inputSize);
		return msclr::interop::marshal_as<String^>(buf);		
	}

	System::Security::SecureString ^ReadLineAsSecureString(void)override {
		auto res = ReadLine();
		auto s = gcnew System::Security::SecureString;
		for each(auto c in res){
			s->AppendChar(c);
		}
		return s;
	}

	void Write(System::String ^ output)override {
		auto str = _marshal_context.marshal_as<PCWSTR>(output);
		g_ExtInstancePtr->Out(str);
	}

	void Write(System::ConsoleColor,System::ConsoleColor,System::String ^ output)override {
		auto str = _marshal_context.marshal_as<PCWSTR>(output);
		g_ExtInstancePtr->Out(str);
	}

	void WriteLine(System::String^ output)override {
		auto str = _marshal_context.marshal_as<PCWSTR>(output);
		g_ExtInstancePtr->Out(L"%s\r\n", str);
	}

	void WriteErrorLine(System::String ^ error)override {
		auto str = _marshal_context.marshal_as<PCWSTR>(error);
		g_ExtInstancePtr->Err(L"ERROR: %s\r\n", str);
	}

	void WriteDebugLine(System::String ^text)override {
		auto str = _marshal_context.marshal_as<PCWSTR>(text);
		g_ExtInstancePtr->Out(L"DEBUG: %s\r\n", str);
	}

	void WriteProgress(__int64,System::Management::Automation::ProgressRecord ^)override {
	}

	void WriteVerboseLine(System::String ^text)override {
		auto str = _marshal_context.marshal_as<PCWSTR>(text);
		g_ExtInstancePtr->Out(L"VERBOSE: %s\r\n", str);
	}

	void WriteDebuggerOutput(System::String ^text, System::String^ prefix){
		WriteDebuggerOutput(prefix + text);
	}

	void WriteDebuggerOutput(System::String ^text){
		auto str = _marshal_context.marshal_as<PCWSTR>(text);
		g_ExtInstancePtr->Out(str);
	}

	void WriteWarningLine(System::String ^)override {}

	System::Collections::Generic::Dictionary<System::String ^,System::Management::Automation::PSObject ^> ^Prompt(System::String ^,System::String ^,System::Collections::ObjectModel::Collection<System::Management::Automation::Host::FieldDescription ^> ^)override {
		throw gcnew  PSNotImplementedException();
	}

	System::Management::Automation::PSCredential ^PromptForCredential(System::String ^,System::String ^,System::String ^,System::String ^)override {
		throw gcnew  PSNotImplementedException();
	}

	System::Management::Automation::PSCredential ^PromptForCredential(System::String ^,System::String ^,System::String ^,System::String ^,System::Management::Automation::PSCredentialTypes,System::Management::Automation::PSCredentialUIOptions)override {
		throw gcnew  PSNotImplementedException();
	}

	int PromptForChoice(System::String ^,System::String ^,System::Collections::ObjectModel::Collection<System::Management::Automation::Host::ChoiceDescription ^> ^,int)override {
		throw gcnew  PSNotImplementedException();
	}


	   
};