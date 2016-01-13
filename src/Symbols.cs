using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Diagnostics.Runtime.Interop;
using static PSExt.ErrorHelper;

namespace PSExt
{
	internal class Symbols
	{
		private readonly IDebugSymbols3 _symbols;

		public Symbols(IDebugSymbols3 symbols)
		{
			_symbols = symbols;
		}

		public IList<SymbolSearchResult> GetMatchingSymbols(string pattern)
		{
			var retval = new List<SymbolSearchResult>(100);
			using (var searcher = new SymbolsSearch(_symbols, pattern)) {			
				SymbolSearchResult res;
				while (searcher.GetNextMatch(out res))
				{
					retval.Add(res);
				}
				return retval;
			}
		}

		public void GetSymbolNameByOffset(ulong offset, ref StringBuilder builder, out ulong displacement)
		{
			uint nameSize;
			var res = _symbols.GetNameByOffsetWide(offset, builder, builder.Capacity, out nameSize, out displacement);
			switch (res)
			{
				case 0: // S_OK					
					return;
				case 1: // S_FALSE					
					builder = new StringBuilder((int)nameSize);
					GetSymbolNameByOffset(offset, ref builder, out displacement);
					return;
				default:
					ErrorHelper.ThrowDebuggerException(res, "IDebugSymbols3.GetNameByOffsetWide");
					return;
			}
		}

		private class SymbolsSearch : IDisposable
		{
			private readonly IDebugSymbols3 _symbols;
			private readonly UInt64 _searchHandle;
			private readonly StringBuilder _builder = new StringBuilder(256);			

			public SymbolsSearch(IDebugSymbols3 symbols, string pattern)
			{
				_symbols = symbols;
				_symbols.StartSymbolMatchWide(pattern, out _searchHandle);
			}

			public bool GetNextMatch(out SymbolSearchResult nextMatch)
			{
				ulong offset;
				uint matchSize;
				int res = _symbols.GetNextSymbolMatchWide(_searchHandle, _builder, _builder.Capacity, out matchSize, out offset);
				switch (res)
				{
					case 0: // S_OK
						nextMatch = new SymbolSearchResult(offset, _builder.ToString());
						return true;
					case 1: // S_FALSE
						_builder.EnsureCapacity((int) matchSize);
						return GetNextMatch(out nextMatch);
					case NoInterface:
						nextMatch = null;
						return false;
					default:
						nextMatch = null;
						return false;
				}				
			}

			public void Dispose()
			{
				_symbols.EndSymbolMatch(_searchHandle);
			}
		}
	}
}