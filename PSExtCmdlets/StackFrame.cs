using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PSExt
{
	public class StackFrame
	{
		public StackFrame(ulong returnOffset, ulong instructionOffset, ulong frameOffset, ushort frameNumber, string name,
			ulong displacement, Callstack callstack)
		{
			ReturnOffset = returnOffset;
			InstructionOffset = instructionOffset;
			FrameOffset = frameOffset;
			FrameNumber = frameNumber;
			Name = name;
			Displacement = displacement;
			Callstack = callstack;
		}

		public ulong ReturnOffset { get; }
		public ulong InstructionOffset { get; }
		public ulong FrameOffset { get; }
		public string Name { get; }
		public ulong Displacement { get; }
		public ushort FrameNumber { get; }
		public Callstack Callstack { get; }

		public override string ToString()
		{
			return $"{Name}+0x{FrameOffset:X}";
		}
	}

	public class Callstack
	{
		public Callstack(List<StackFrame> frames)
		{
			Frames = frames;
		}

		public List<StackFrame> Frames { get; }

		public bool Matches(string pattern)
		{
			return Frames.Any(f => Regex.IsMatch(f.Name, pattern));
		}

		public bool MatchesAll(string[] patterns)
		{
			return patterns.All(Matches);
		}

		public bool MatchesAny(string[] patterns)
		{
			return patterns.Any(Matches);
		}

		public bool Contains(string pattern)
		{
			return Frames.Any(f => f.Name.IndexOf(pattern, StringComparison.Ordinal) != -1);
		}

		public bool ContainsAll(string[] patterns)
		{
			return patterns.All(Contains);
		}

		public bool ContainsAny(string[] patterns)
		{
			return patterns.Any(Contains);
		}
	}
}