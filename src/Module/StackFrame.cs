using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PSExt
{
	public class StackFrame
	{
		public StackFrame(ulong returnOffset, ulong instructionOffset, ulong frameOffset, ushort frameNumber, string name,
			ulong displacement, DebugThread thread)
		{
			ReturnOffset = returnOffset;
			InstructionOffset = instructionOffset;
			FrameOffset = frameOffset;
			FrameNumber = frameNumber;
			Name = string.Intern(name);
			var sep = name.IndexOf('!');
			sep = (sep == -1) ? 0 : sep;
			Module = string.Intern(name.Substring(0, sep));
			Function = name.Substring(sep + 1);
			Displacement = displacement;
			Thread = thread;
		}

		public ulong ReturnOffset { get; }
		public ulong InstructionOffset { get; }
		public ulong FrameOffset { get; }
		public string Name { get; }
		public string Module { get; }
		public string Function { get; }
		public ulong Displacement { get; }
		public ushort FrameNumber { get; }
		public DebugThread Thread { get; }

		public bool Matches(string pattern)
		{
			return Regex.IsMatch(Name, pattern);
		}
		
		public bool Contains(string pattern)
		{
			return Name.IndexOf(pattern, StringComparison.Ordinal) != -1;
		}

		public override string ToString()
		{
			return $"{Name}+0x{Displacement:X}";
		}
	}
}