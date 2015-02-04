#pragma once

ref class MethodInvocationInfo;

ref class DebuggerDispatcher{
	
	MethodInvocationInfo^ _invocationInfo;	
	System::Threading::ManualResetEvent^ _doCallEvent;
	System::Threading::AutoResetEvent^ _doReturn;	
	System::Threading::Thread^ _dispatchThread;	
	System::Object^ _lock;

	MethodInvocationInfo^ GetMethodInvocation(System::Type^ type, System::Object^ instance, System::String^ methodName);
	MethodInvocationInfo^ GetMethodInvocation(System::Type^ type, System::Object^ instance, System::String^ methodName, System::Object^ arg1);
	
	System::Object^ InvokeFunction(MethodInvocationInfo^ invocationInfo);
	
	DebuggerDispatcher();


public:
	static DebuggerDispatcher^ instance;
	static property DebuggerDispatcher^ Instance{ DebuggerDispatcher^ get();};

	System::Object^ InvokeFunction(System::Type^ type, System::String^ methodName);
	System::Object^ InvokeFunction(System::Type^ type, System::String^ methodName, Object^ arg1);

	System::Object^ InvokeFunction(System::Type^ type, Object^ instance, System::String^ methodName);
	System::Object^ InvokeFunction(System::Type^ type, Object^ instance, System::String^ methodName, Object^ arg1);

	void Start(System::Threading::WaitHandle^ pipelineCompleted);

	bool DispatchRequired();

};
