using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace PSExt
{
	public interface IMethodInvocationInfo
	{
		void Invoke();
		object GetResult();
	}

	public class MethodInvocationInfo : IMethodInvocationInfo
	{
		private readonly object[] _args;
		private readonly MethodInfo _mi;
		private readonly object _this;
		private object _res;
		private Exception _exception;


		public MethodInvocationInfo(MethodInfo mi, object that, object[] args)
		{
			_mi = mi;
			_this = that;
			_args = args;
			if (mi == null)
			{
				throw new ArgumentNullException(nameof(mi), "The method was not found");
			}
		}

		public MethodInvocationInfo(MethodInfo mi, object that, object arg1)
		{
			_mi = mi;
			_this = that;
			_args = new[] {arg1};
			if (mi == null)
			{
				throw new ArgumentNullException(nameof(mi), "The method was not found");
			}
		}

		public MethodInvocationInfo(MethodInfo mi, object that, object arg1, object arg2)
		{
			_mi = mi;
			_this = that;
			_args = new[] {arg1, arg2};
			if (mi == null)
			{
				throw new ArgumentNullException(nameof(mi), "The method was not found");
			}
		}

		public void Invoke()
		{
			try
			{
				_res = _mi.Invoke(_this, _args);
			}
			catch (TargetInvocationException te)
			{
				_exception = te.InnerException;
			}
			catch (Exception exception)
			{
				_exception = exception;	
			}
		}

		public object GetResult()
		{
			if (_exception != null)
			{
				throw _exception;
			}
			return _res;
		}

		private static MethodInfo GetMethodInfo(Type type, string methodName)
		{
			var retval = type.GetMethod(methodName);
			if (retval == null)
			{
				throw new ArgumentOutOfRangeException(nameof(methodName), methodName, "No method was found with the specified name.");
			}
			return retval;
		}

		public static MethodInvocationInfo GetMethodInvocation(Type type, object instance, string methodName)
		{
			return new MethodInvocationInfo(
				GetMethodInfo(type, methodName),
				instance,
				new object[0]);
		}

		public static MethodInvocationInfo GetMethodInvocation(Type type, object instance, string methodName, object arg1)
		{
			return new MethodInvocationInfo(
				GetMethodInfo(type, methodName),
				instance,
				new[] {arg1});
		}
	}

	public interface IEventProcessor
	{
		void ProcessEvents(WaitHandle pipelineCompleted);
	}

	public interface IDebugFunctionDispatch
	{
		bool DispatchRequired();
		object InvokeFunction(MethodInvocationInfo invocationInfo);
	}

	public class DebuggerDispatcher : IEventProcessor, IDebugFunctionDispatch
	{
		private static DebuggerDispatcher _instance;
		private readonly ManualResetEvent _doCallEvent;
		private readonly AutoResetEvent _doReturn;
		private readonly object _lock;
		private Thread _dispatchThread;
		private IMethodInvocationInfo _invocationInfo;


		public DebuggerDispatcher()
		{
			_doCallEvent = new ManualResetEvent(false);
			_doReturn = new AutoResetEvent(false);
			_lock = new object();
		}

		public static DebuggerDispatcher Instance => _instance ?? (_instance = new DebuggerDispatcher());


		public object InvokeFunction(MethodInvocationInfo invocationInfo)
		{
			lock (_lock)
			{
				_invocationInfo = invocationInfo;
				_doCallEvent.Set();
				_doReturn.WaitOne();
				_doReturn.Reset();
				var res = _invocationInfo.GetResult();
				_invocationInfo = null;
				return res;
			}
		}

		public void ProcessEvents(WaitHandle pipelineCompleted)
		{
			_dispatchThread = Thread.CurrentThread;
			var handles = new WaitHandle[2];
			handles[0] = _doCallEvent;
			handles[1] = pipelineCompleted;
			var res = 0;

			do
			{
				res = WaitHandle.WaitAny(handles);
				if (res == 0)
				{
					
					_invocationInfo.Invoke();
					
					_doCallEvent.Reset();
					_doReturn.Set();
				}
			} while (res == 0);
		}

		public bool DispatchRequired()
		{
			return Thread.CurrentThread != _dispatchThread;
		}
	}
}