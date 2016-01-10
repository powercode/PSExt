using System.Diagnostics;
using System.Threading;

namespace PSExt
{
	public interface IProgram : IEventProcessor
	{
		bool ShouldExit { get; set; }
		int ExitCode { get; set; }		
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

	class Program : IProgram
	{
		private IDebugger _debugger;		
		private readonly IEventProcessor _eventProcessor;

		public Program(IDebugger debugger, IEventProcessor eventProcessor)
		{
			_debugger = debugger;			
			_eventProcessor = eventProcessor;
		}

		public bool ShouldExit { get; set; }
		public int ExitCode { get; set; }
		public void ProcessEvents(WaitHandle waitHandle)
		{
			_eventProcessor.ProcessEvents(waitHandle);
		}

		
	}
}