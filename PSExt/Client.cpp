
#include "Client.h"
#include "engextcpp.hpp"

using namespace System;


String^ Client::ExecuteCommand(String^ command){	
	auto cmd = _marshal_context.marshal_as<PCSTR>(command);
	ExtCaptureOutputW outputCapture;
	outputCapture.Execute(cmd);
	auto res = outputCapture.GetTextNonNull();
	auto mres = msclr::interop::marshal_as<String^>(res);
	return mres;
}


String^ Client::ReadLine(){	
	wchar_t buf[256];
	ULONG inputSize = 0;
	g_ExtInstancePtr->m_Control4->InputWide(buf, 256, &inputSize);
	return msclr::interop::marshal_as<String^>(buf);
}

void Client::Write(String ^ output) {
	auto str = _marshal_context.marshal_as<PCWSTR>(output);
	g_ExtInstancePtr->Out(str);
}
