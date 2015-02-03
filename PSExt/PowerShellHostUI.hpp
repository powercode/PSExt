#pragma once
#include "PowerShellHostRawUI.hpp"
#include <msclr\marshal.h>
#include "DebuggerDispatcher.h"


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
		return DebuggerDispatcher::Instance->ReadLine();		
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
		DebuggerDispatcher::Instance->Write(output);
	}

	void Write(System::ConsoleColor,System::ConsoleColor,System::String ^ output)override {
		Write(output);
	}

	void WriteLine(System::String^ output)override {
		Write(output + System::Environment::NewLine);
	}

	void WriteErrorLine(System::String ^ error)override {
		WritePrefixedLine("Error", error);
	}

	void WriteDebugLine(System::String ^text)override {
		WritePrefixedLine("Debug", text);
	}

	void WriteProgress(__int64,System::Management::Automation::ProgressRecord ^)override {
	}

	void WriteVerboseLine(System::String ^text)override {
		WritePrefixedLine("Verbose", text);
	}

	void WriteWarningLine(System::String ^text)override {
		WritePrefixedLine("Warning", text);
	}

	void WritePrefixedLine(String^ prefix, System::String^ output) {
		Write(System::String::Format("{0}: {1}{2}", prefix, output, System::Environment::NewLine));
	}

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