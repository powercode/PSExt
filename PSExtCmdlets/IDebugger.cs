using System.Collections.Generic;

namespace PSExt
{
	public interface IDebugger
	{
		string ExecuteCommand(string command);
		List<BreakpointData> GetBreakpoints();
		string ReadLine();
		void Write(string value);		
	}
}
