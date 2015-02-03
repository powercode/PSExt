#pragma once
#include <functional>
#include <msclr/marshal.h>
#include <msclr/lock.h>
using namespace System::Collections::Concurrent;
using namespace System;

ref class DebuggerDispatcher{
	
	ref class  MethodInvocationInfo{
		System::Reflection::MethodInfo^ _mi;
		Object^ _this;
		Object^ _res;
		array<Object^>^ _args;
	public:
		MethodInvocationInfo(System::Reflection::MethodInfo^ mi, Object^ that, array<Object^>^ args)
			: _mi(mi)
			, _this(that)
			, _res(nullptr)
			, _args(args)
		{
		}
		MethodInvocationInfo(System::Reflection::MethodInfo^ mi, Object^ that, Object^ arg1)
			: _mi(mi)
			, _this(that)
			, _res(nullptr)
			, _args(gcnew array<Object^>(1))
		{
			_args[0] = arg1;
		}
		MethodInvocationInfo(System::Reflection::MethodInfo^ mi, Object^ that, Object^ arg1, Object^ arg2)
			: _mi(mi)
			, _this(that)
			, _res(nullptr)
			, _args(gcnew array<Object^>(2))
		{			
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

	MethodInvocationInfo^ _invocationInfo;	
	System::Threading::ManualResetEvent^ _doCallEvent;
	System::Threading::AutoResetEvent^ _doReturn;	
	System::Threading::Thread^ _dispatchThread;
	msclr::interop::marshal_context _marshal_context;

	MethodInvocationInfo^ GetMethodInvocation(System::String^ methodName){
		return gcnew MethodInvocationInfo(
			GetType()->GetMethod(methodName),
			this,
			gcnew array<Object^>(0));
	}
	MethodInvocationInfo^ GetMethodInvocation(System::String^ methodName, Object^ arg1){
		return gcnew MethodInvocationInfo(
			GetType()->GetMethod(methodName),
			this,
			arg1);
	}

	Object^ _lock;

	Object^ InvokeFunction(MethodInvocationInfo^ invocationInfo){
		msclr::lock lock(_lock);
		_invocationInfo = invocationInfo;
		_doCallEvent->Set();
		_doReturn->WaitOne();
		_doReturn->Reset();
		auto res =_invocationInfo->GetResult();
		_invocationInfo = nullptr;
		return res;
	}

	DebuggerDispatcher(){
		_doCallEvent = gcnew System::Threading::ManualResetEvent(false);
		_doReturn = gcnew System::Threading::AutoResetEvent(false);
		_lock = gcnew Object();
	}


public:
	static DebuggerDispatcher^ instance;
	static property DebuggerDispatcher^ Instance{
		DebuggerDispatcher^ get(){
			if (instance == nullptr){
				instance = gcnew DebuggerDispatcher();
			}
			return instance;
		}
	}
	
	Object^ InvokeFunction(String^ methodName){
		return InvokeFunction(GetMethodInvocation(methodName));
	}

	Object^ InvokeFunction(String^ methodName, Object^ arg1){
		return InvokeFunction(GetMethodInvocation(methodName, arg1));
	}

	void Start(System::Threading::WaitHandle^ pipelineCompleted){
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

	bool DispatchRequired(){
		return _dispatchThread != System::Threading::Thread::CurrentThread;
	}

	String^ ExecuteCommand(String^ command){		
		if (DispatchRequired()){			
			return (String^)InvokeFunction("ExecuteCommand", command);			
		}

		auto cmd = _marshal_context.marshal_as<PCSTR>(command);
		ExtCaptureOutputW outputCapture;
		outputCapture.Execute(cmd);
		auto res = outputCapture.GetTextNonNull();
		auto mres = msclr::interop::marshal_as<String^>(res);
		return mres;
	}

	String^ ReadLine(){
		if (DispatchRequired()){
			return (String^) InvokeFunction("ReadLine");			
		}				
		
		wchar_t buf[256];
		ULONG inputSize = 0;
		g_ExtInstancePtr->m_Control4->InputWide(buf, 256, &inputSize);
		return msclr::interop::marshal_as<String^>(buf);
	}
	
	void Write(System::String ^ output) {
		if (DispatchRequired()){						
			InvokeFunction("Write", output);
			return;			
		}
		auto str = _marshal_context.marshal_as<PCWSTR>(output);
		g_ExtInstancePtr->Out(str);
	}		

};

enum BreakpointKind{
	Code,
	Data,
};

enum DataBreakpointKind{
	Read,
	Write,
	ReadWrite = Read | Write,
	Execute,
	IO
};

ref class BreakpointData{

};

ref class DebuggerBreakpoint{

	System::Collections::Generic::List<BreakpointData^>^ GetBreakpoints(){
		return nullptr;
	}

};