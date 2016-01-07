using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace PSExt
{
	public class StackFrame
	{		
		public UInt64 ReturnOffset { get; }
		public UInt64 InstructionOffset { get; }
		public UInt64 FrameOffset { get; }		
		public string Name { get; }
		public UInt64 Displacement { get; }
		public UInt16 FrameNumber { get; }
		public Callstack Callstack { get; }


		public StackFrame(UInt64 returnOffset, ulong instructionOffset, ulong frameOffset, ushort frameNumber, string name, UInt64 displacement, Callstack callstack)
		{
			ReturnOffset = returnOffset;
			InstructionOffset = instructionOffset;
			FrameOffset = frameOffset;
			FrameNumber = frameNumber;
			Name = name;
			Displacement = displacement;
			Callstack = callstack;
		}

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