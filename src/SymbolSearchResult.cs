using System;

namespace PSExt
{
	class SymbolSearchResult
	{
		public UInt64 Offset;
		public String Name;

		public SymbolSearchResult(ulong offset, string name)
		{
			Offset = offset;
			Name = name;
		}
	}
}