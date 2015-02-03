#pragma once
#include "engextcpp.hpp"
#include <msclr\marshal.h>
#include "DebuggerDispatcher.h"

using namespace System;

ref class SimpleDbgModule{
public:
	property System::UInt64 Start;
	property System::UInt64 End;
	property System::String^ ModuleName;
	SimpleDbgModule(UInt64 start, UInt64 end, String^ moduleName)		
	{		
		Start = start;
		End = end;
		ModuleName = moduleName;
	}

	String^ ToString() override{
		return ModuleName;
	}
};

[CmdletAttribute(VerbsCommon::Get, "DbgModule")]
[OutputType(SimpleDbgModule::typeid)]
ref class GetDebuggerModuleCommand: PSCmdlet{	
	static System::Text::RegularExpressions::Regex^ _pattern = gcnew System::Text::RegularExpressions::Regex("(?<f>\\S+)\\s(?<t>\\S+)\\s+(?<m>\\S+)");
public:

	void EndProcessing() override{

		auto res = DebuggerDispatcher::Instance->ExecuteCommand("lm");
		
		for each(auto line in res->Split(gcnew array<wchar_t>(2){ '\r', '\n' }, System::StringSplitOptions::RemoveEmptyEntries)){
			auto match = _pattern->Match(line);
			if (match->Success){
				auto start = Convert::ToUInt64(match->Groups[1]->Value->Remove(8,1), 16);
				auto end = Convert::ToUInt64(match->Groups[2]->Value->Remove(8, 1), 16);
				auto name = match->Groups[3]->Value;
				WriteObject(gcnew SimpleDbgModule(start,end,name));
			}
		}
	}
};