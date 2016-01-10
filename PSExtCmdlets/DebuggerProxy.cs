using System.Collections.Generic;
using System.Dynamic;

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
		public DebuggerProxy(IDebugger debugger)
		{
			_proxy = new DynamicDebuggerProxy(debugger);
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

		public DebugThread GetCallstack()
		{
			return _proxy.GetCallstack();
		}

		private class DynamicDebuggerProxy : DynamicObject
		{
			private readonly DebuggerDispatcher _dispatcher;
			private readonly IDebugger _proxy;

			public DynamicDebuggerProxy(IDebugger proxy)
			{
				_proxy = proxy;
				_dispatcher = DebuggerDispatcher.Instance;
			}

			public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
			{
				try
				{
					var mi = typeof (IDebugger).GetMethod(binder.Name);
					if (_dispatcher.DispatchRequired())
					{
						result = _dispatcher.InvokeFunction(new MethodInvocationInfo(mi, _proxy, args));
						return true;
					}
					result = mi.Invoke(_proxy, args);
					return true;
				}
				catch
				{
					result = null;
					return false;
				}
			}
		}
	}
}