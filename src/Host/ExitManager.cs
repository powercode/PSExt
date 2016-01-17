using System.Threading;

namespace PSExt
{
	public class ExitManager 
	{			
		public bool ShouldExit { get; set; }
		public int ExitCode { get; set; }				
	}
}