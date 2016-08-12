using System;
using System.Collections.Generic;
using System.Management.Automation;
using DebugData;

namespace PSExt
{
	public interface IDebugger : IDisposable
	{
		string ExecuteCommand(string command);
		string ReadLine();
		void Write(string value);
		IList<BreakpointData> GetBreakpoints();
		IList<BreakpointData> AddBreakpoints(BreakpointData data);
		IList<ModuleData> GetModules();
		IList<DebugThread> GetCallstack(bool all);
		IList<SymbolValue> GetVariables(StackFrame frame, int levels);
		void SetSymbolPath(string symbolPath);
		void ReloadSymbols();
		void EndDumpSession();
	}
}