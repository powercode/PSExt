using System;

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


		public StackFrame(UInt64 returnOffset, ulong instructionOffset, ulong frameOffset, ushort frameNumber, string name, UInt64 displacement)
		{
			ReturnOffset = returnOffset;
			InstructionOffset = instructionOffset;
			FrameOffset = frameOffset;
			FrameNumber = frameNumber;
			Name = name;
			Displacement = displacement;
		}

		public override string ToString()
		{
			return $"{Name}+0x{FrameOffset:X}";
		}
	}
}