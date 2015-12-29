#include "NativeCallstack.h"
#include "engextcpp.hpp"


StackVector GetNativeStack()
{
	StackVector stackVector;
	stackVector.resize(256);
	ULONG filled;
	HRESULT status = E_FAIL;

	if ((status = g_Ext->m_Control5->GetStackTraceEx(0, 0, 0, &stackVector[0], ULONG(stackVector.size()), OUT &filled)) != S_OK)
	{
		g_Ext->ThrowRemote(status, "Unable to get callstack.");
	}
	stackVector.resize(filled);
	return stackVector;

}
