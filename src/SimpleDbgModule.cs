namespace PSExt
{
	public class SimpleDbgModule
	{
		public SimpleDbgModule(ulong start, ulong end, string moduleName)
		{
			Start = start;
			End = end;
			ModuleName = moduleName;
		}

		public ulong Start { get; private set; }
		public ulong End { get; private set; }
		public string ModuleName { get; }

		public override string ToString()
		{
			return ModuleName;
		}
	}
}