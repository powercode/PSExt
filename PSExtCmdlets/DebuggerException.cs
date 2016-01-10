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
}