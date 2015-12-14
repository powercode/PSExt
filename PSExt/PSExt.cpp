// PSExt.cpp : Defines the exported functions for the DLL application.
//
//----------------------------------------------------------------------------
//
// extcpp.cpp
//
// EngExtCpp-style extension sample.
//
// Copyright (C) Microsoft Corporation, 2005.
//
//----------------------------------------------------------------------------

#include "engextcpp.hpp"
#include "PowerShellCommands.h"
#include "CmdletLoader.h"
//----------------------------------------------------------------------------
//
// Base extension class.
// Extensions derive from the provided ExtExtension class.
//
// The standard class name is "Extension".  It can be
// overridden by providing an alternate definition of
// EXT_CLASS before including engextcpp.hpp.
//
//----------------------------------------------------------------------------


class EXT_CLASS : public ExtExtension
{
public:	
	EXT_COMMAND_METHOD(ps);	
	EXT_COMMAND_METHOD(test);



	HRESULT Initialize() override{
		LoadCmdletAssembly();
		return InitializePowerShell();
	}

	void Uninitialize() override{

		UninitializePowerShell();
	}
};

// EXT_DECLARE_GLOBALS must be used to instantiate
// the framework's assumed globals.
EXT_DECLARE_GLOBALS();

//----------------------------------------------------------------------------
//
// ps extension command.
//
// This command uses the framework's built-in OS
// data querying methods to do a walk over the
// user-mode loaded module list.
//
// The argument string means:
//
//   {;          - No name for the current (first) argument.
//   e,          - The argument is an expression.
//   o,          - The argument is optional.
//   d=@$peb;    - The argument's default expression is @$peb.
//   peb;        - The argument's short description is "peb".
//   PEB address - The argument's long description.
//   }           - No further arguments.
//
// This extension has a single, optional argument that
// is an expression for the PEB address.
//
//----------------------------------------------------------------------------



EXT_COMMAND(ps,
	"Invokes a powershell command for the debugger",
	"{{custom}}{{s:cmd}}{{l:a powershell pipeline to execute}}")
{
	auto args = GetRawArgStr();
	InvokePowerShellCommand(args);	
}

#include "Symbols.h"

EXT_COMMAND(test,
	"Test the command under development",
	"{{custom}}{{s:cmd}}{{l:a your custom args if needed}}")
{	
	auto syms= Symbols::GetMatchingSymbols(L"mem*");	
	for (auto& s : syms){
		g_Ext->Out(L"0x%p: %s\r\n", s.Offset, s.Name.c_str());
	}
}

