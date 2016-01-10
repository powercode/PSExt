namespace PSExt
{
	public class StackFrame
	{
		public StackFrame(ulong returnOffset, ulong instructionOffset, ulong frameOffset, ushort frameNumber, string name,
			ulong displacement, DebugThread callstack)
		{
			ReturnOffset = returnOffset;
			InstructionOffset = instructionOffset;
			FrameOffset = frameOffset;
			FrameNumber = frameNumber;
			Name = name;
			Displacement = displacement;
			Thread = callstack;
		}

		public ulong ReturnOffset { get; }
		public ulong InstructionOffset { get; }
		public ulong FrameOffset { get; }
		public string Name { get; }
		public ulong Displacement { get; }
		public ushort FrameNumber { get; }
		public DebugThread Thread { get; }

		public override string ToString()
		{
			return $"{Name}+0x{Displacement:X}";
		}
	}
}