using System;
using System.ComponentModel;

namespace PSExt
{
	internal class DebuggerException : Win32Exception
	{
		public string FailingMethod { get; set; }
		public int StatusCode { get; set; }

		public DebuggerException(int statusCode, string failingMethod) : base(statusCode)
		{
			FailingMethod = failingMethod;
		}		
	}

	internal class DebuggerInvocationException : Exception
	{	
		public DebuggerInvocationException(Exception exception) : base(exception.Message, exception)
		{			
		}

		public override string StackTrace => InnerException?.StackTrace ?? base.StackTrace;
	}
}