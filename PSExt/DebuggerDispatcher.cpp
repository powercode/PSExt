#include "DebuggerDispatcher.h"
#include "engextcpp.hpp"
#include <functional>
#include <msclr/marshal.h>
#include <msclr/lock.h>

using namespace System::Collections::Generic;
using namespace System;
using namespace System::Reflection;

ref class  MethodInvocationInfo{
	MethodInfo^ _mi;
	Object^ _this;
	Object^ _res;
	array<Object^>^ _args;

	
public:
	MethodInvocationInfo(MethodInfo^ mi, Object^ that, array<Object^>^ args)
		: _mi(mi)
		, _this(that)
		, _res(nullptr)
		, _args(args)
	{
		if (mi == nullptr){
			throw gcnew ArgumentNullException("mi", "The method was not found");
		}
	}
	MethodInvocationInfo(System::Reflection::MethodInfo^ mi, Object^ that, Object^ arg1)
		: _mi(mi)
		, _this(that)
		, _res(nullptr)
		, _args(gcnew array<Object^>(1))
	{
		if (mi == nullptr){
			throw gcnew ArgumentNullException("mi", "The method was not found");
		}
		_args[0] = arg1;
	}
	MethodInvocationInfo(System::Reflection::MethodInfo^ mi, Object^ that, Object^ arg1, Object^ arg2)
		: _mi(mi)
		, _this(that)
		, _res(nullptr)
		, _args(gcnew array<Object^>(2))
	{
		if (mi == nullptr){
			throw gcnew ArgumentNullException("mi", "The method was not found");
		}
		_args[0] = arg1;
		_args[1] = arg2;
	}

	void Invoke(){
		
		_res = _mi->Invoke(_this, _args);
	}

	Object^ GetResult(){
		return _res;
	}
};

DebuggerDispatcher::DebuggerDispatcher(){
	_doCallEvent = gcnew System::Threading::ManualResetEvent(false);
	_doReturn = gcnew System::Threading::AutoResetEvent(false);
	_lock = gcnew Object();
}

MethodInfo^ DebuggerDispatcher::GetMethodInfo(Type^ type, String^ methodName){
	auto retval = type->GetMethod(methodName);
	if (retval == nullptr){
		throw gcnew ArgumentOutOfRangeException("methodName", methodName, "No method was found with the specified name.");
	}
	return retval;
}


MethodInvocationInfo^ DebuggerDispatcher::GetMethodInvocation(Type^ type, Object^ instance, System::String^ methodName){
	return gcnew MethodInvocationInfo(
		GetMethodInfo(type, methodName),
		instance,
		gcnew array<Object^>(0));
}

MethodInvocationInfo^ DebuggerDispatcher::GetMethodInvocation(Type^ type, Object^ instance, System::String^ methodName, Object^ arg1){
	return gcnew MethodInvocationInfo(
		GetMethodInfo(type, methodName),
		instance,
		arg1);
}

Object^ DebuggerDispatcher::InvokeFunction(Type^ type, String^ methodName){
	return InvokeFunction(GetMethodInvocation(type, nullptr, methodName));
}

Object^ DebuggerDispatcher::InvokeFunction(Type^ type, String^ methodName, Object^ arg1){
	return InvokeFunction(GetMethodInvocation(type, nullptr, methodName, arg1));
}


Object^ DebuggerDispatcher::InvokeFunction(Type^ type, Object^ instance, String^ methodName){
	return InvokeFunction(GetMethodInvocation(type, instance, methodName));
}

Object^ DebuggerDispatcher::InvokeFunction(Type^ type, Object^ instance, String^ methodName, Object^ arg1){
	return InvokeFunction(GetMethodInvocation(type, instance, methodName, arg1));
}

DebuggerDispatcher^ DebuggerDispatcher::Instance::get(){	
	if (instance == nullptr){
		instance = gcnew DebuggerDispatcher();
	}
	return instance;	
}

System::Object^ DebuggerDispatcher::InvokeFunction(MethodInvocationInfo^ invocationInfo){
	msclr::lock lock(_lock);
	_invocationInfo = invocationInfo;
	_doCallEvent->Set();
	_doReturn->WaitOne();
	_doReturn->Reset();
	auto res = _invocationInfo->GetResult();
	_invocationInfo = nullptr;
	return res;
}

void DebuggerDispatcher::Start(System::Threading::WaitHandle^ pipelineCompleted){
	_dispatchThread = System::Threading::Thread::CurrentThread;
	array<System::Threading::WaitHandle^>^ handles = gcnew array<System::Threading::WaitHandle^>(2);
	handles[0] = _doCallEvent;
	handles[1] = pipelineCompleted;
	int res = 0;

	do{
		res = System::Threading::WaitHandle::WaitAny(handles);
		if (res == 0){
			_invocationInfo->Invoke();
			_doCallEvent->Reset();
			_doReturn->Set();
		}
	} while (res == 0);
}

bool DebuggerDispatcher::DispatchRequired(){
	return _dispatchThread != System::Threading::Thread::CurrentThread;
}


