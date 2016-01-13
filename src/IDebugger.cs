using System.Collections.Generic;

namespace PSExt
{
	public interface IDebugger
	{
		string ExecuteCommand(string command);
		string ReadLine();
		void Write(string value);
		IList<BreakpointData> GetBreakpoints();
		IList<BreakpointData> AddBreakpoints(BreakpointData data);
		IList<ModuleData> GetModules();
		IList<DebugThread> GetCallstack(bool all);
	}
}