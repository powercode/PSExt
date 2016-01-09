using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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
		private readonly MethodInfo _mi;
		private readonly object _this;
		private readonly object[] _args;
		private object _res;

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
				new[] { arg1 });
		}


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
			_args = new[] { arg1 };
			if (mi == null)
			{
				throw new ArgumentNullException(nameof(mi), "The method was not found");
			}
		}

		public MethodInvocationInfo(MethodInfo mi, object that, object arg1, object arg2)
		{
			_mi = mi;
			_this = that;
			_args = new[] { arg1, arg2 };
			if (mi == null)
			{
				throw new ArgumentNullException(nameof(mi), "The method was not found");
			}
		}

		public void Invoke()
		{
			_res = _mi.Invoke(_this, _args);
		}

		public object GetResult()
		{
			return _res;
		}
	}

	public class DebuggerDispatcher
	{
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

		public void Start(WaitHandle pipelineCompleted)
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

		private static DebuggerDispatcher _instance;
		public static DebuggerDispatcher Instance => _instance ?? (_instance = new DebuggerDispatcher());
	}
}
