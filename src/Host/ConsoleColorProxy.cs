using System;

namespace PSExt.Host
{
	public class ConsoleColorProxy
	{
		public ConsoleColor DebugBackgroundColor { get; set; }
		public ConsoleColor DebugForegroundColor { get; set; }
		public ConsoleColor ErrorBackgroundColor { get; set; }
		public ConsoleColor ErrorForegroundColor { get; set; }
		public ConsoleColor ProgressBackgroundColor { get; set; }
		public ConsoleColor ProgressForegroundColor { get; set; }
		public ConsoleColor VerboseBackgroundColor { get; set; }
		public ConsoleColor VerboseForegroundColor { get; set; }
		public ConsoleColor WarningBackgroundColor { get; set; }
		public ConsoleColor WarningForegroundColor { get; set; }
	}
}