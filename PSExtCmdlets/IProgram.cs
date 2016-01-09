using System.Threading;

namespace PSExt
{
	public interface IProgram
	{
		bool ShouldExit { get; set; }
		int ExitCode { get; set; }
		void ProcessEvents(WaitHandle waitHandle);
	}

	public interface IBreakpoints
	{
	}

	public interface IDebuggerClient
	{
	}

	public interface ICallstack
	{
	}

	public interface IModule
	{
	}
}