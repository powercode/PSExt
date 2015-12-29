#include "Callstack.h"
#include "NativeCallstack.h"
#include "Symbols.h"
#include <msclr/marshal.h>
using namespace msclr::interop;

using namespace PSExt;
using namespace System;
using namespace System::Collections::Generic;

StackFrame^ ToStackFrame(const DEBUG_STACK_FRAME_EX& frame)
{
	UNREFERENCED_PARAMETER(frame);
	ULONG64 displacement = 0;
	auto name = Symbols::GetNameByOffset(frame.InstructionOffset, OUT displacement);
	String^ nameStr = marshal_as<String^>(name.c_str());
	return gcnew StackFrame(frame.ReturnOffset, frame.InstructionOffset, frame.FrameOffset, USHORT(frame.FrameNumber), nameStr, displacement);
}



IList<StackFrame^>^ Callstack::GetCallstacks()
{
	auto stackFrames = GetNativeStack();
	List<StackFrame^>^ res = gcnew List<StackFrame^>((int)stackFrames.size());
	for (auto& frame : stackFrames)
	{
		res->Add(ToStackFrame(frame));
	}
	return res;
}
