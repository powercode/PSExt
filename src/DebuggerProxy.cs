using System.Collections.Generic;
using System.Dynamic;
using System.Management.Automation;

namespace PSExt
{
	/// <summary>
	///     Proxy to debugger responsible for invoking the calls from PowerShell on the debugger thread.
	///     DynamicDebuggerProxy helper does this and delegates to the real debugger.
	/// </summary>
	public class DebuggerProxy : IDebugger
	{
		private readonly dynamic _proxy;

		/// <summary>
		///     Creates a new instance of the debugger proxy
		/// </summary>
		/// <param name="debugger">the real native debugger to delegate calls to</param>
		/// <param name="debugFunctionDispatch"></param>
		public DebuggerProxy(IDebugger debugger, IDebugFunctionDispatch debugFunctionDispatch)
		{
			_proxy = new DynamicDebuggerProxy(debugger, debugFunctionDispatch);
		}

		public string ExecuteCommand(string command)
		{
			return _proxy.ExecuteCommand(command);
		}

		public string ReadLine()
		{
			return _proxy.ReadLine();
		}

		public void Write(string value)
		{
			_proxy.Write(value);
		}

		public IList<BreakpointData> GetBreakpoints()
		{
			return _proxy.GetBreakpoints();
		}

		public IList<BreakpointData> AddBreakpoints(BreakpointData data)
		{
			return _proxy.AddBreakpoints(data);
		}

		public IList<ModuleData> GetModules()
		{
			return _proxy.GetModules();
		}

		public IList<DebugThread> GetCallstack(bool all)
		{
			return _proxy.GetCallstack(all);
		}

		public IList<SymbolValue> GetVariables(StackFrame frame, int levels)
		{
			return _proxy.GetStackFrame(frame, levels);
		}

		private class DynamicDebuggerProxy : DynamicObject
		{			
			private readonly IDebugger _proxy;
			private readonly IDebugFunctionDispatch _debugFunctionDispatch;

			public DynamicDebuggerProxy(IDebugger proxy, IDebugFunctionDispatch debugFunctionDispatch)
			{
				_proxy = proxy;
				_debugFunctionDispatch = debugFunctionDispatch;				
			}

			public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
			{									
				var mi = typeof (IDebugger).GetMethod(binder.Name);
				if (_debugFunctionDispatch.DispatchRequired())
				{						
					result = _debugFunctionDispatch.InvokeFunction(new MethodInvocationInfo(mi, _proxy, args));
					return true;
				}
				result = mi.Invoke(_proxy, args);
				return true;			
			}
		}
	}
}