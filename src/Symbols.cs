using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using DebugData;
using Microsoft.Diagnostics.Runtime.Interop;
using static PSExt.ErrorHelper;

namespace PSExt
{
	internal class Symbols
	{
		private readonly IDebugSymbols5 _symbols;

		public Symbols(IDebugSymbols5 symbols)
		{
			_symbols = symbols;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct LangAndCodepage
		{
			public short lang;
			public short codepage;

			public override string ToString()
			{
				return $"{lang:x4}{codepage:x4}";
			}
		}

		private unsafe string GetModuleVersionItemInfo(uint moduleIndex, string itemName)
		{			
			byte[] buffer = new byte[256];			
			uint verInfoSize;
			LangAndCodepage[] translation = new LangAndCodepage[5];
			fixed (byte* buf = buffer)
			fixed (LangAndCodepage* tb = translation) {				
				uint tsSize;
				var res = _symbols.GetModuleVersionInformationWide(moduleIndex, 0, "\\VarFileInfo\\Translation", new IntPtr(tb), sizeof(LangAndCodepage)*translation.Length, out tsSize);
				res = _symbols.GetModuleVersionInformationWide(moduleIndex, 0, $"\\StringFileInfo\\{translation[0]}\\{itemName}", new IntPtr(buf), 
					buffer.Length, out verInfoSize);
				if (res != 0)
				{
					return String.Empty;
				}
			}

			return Encoding.Unicode.GetString(buffer, 0, (int) verInfoSize - 2);
					
		}

		public ModuleVersionInfo GetModuleVersionInfo(uint moduleIndex)
		{
			var productVersion = GetModuleVersionItemInfo(moduleIndex, "ProductVersion");
			var fileVersion = GetModuleVersionItemInfo(moduleIndex, "FileVersion");
			var productName = GetModuleVersionItemInfo(moduleIndex, "ProductName");
			var company = GetModuleVersionItemInfo(moduleIndex, "CompanyName");
			var desc = GetModuleVersionItemInfo(moduleIndex, "FileDescription");
			var comments = GetModuleVersionItemInfo(moduleIndex, "Comments");
			
			return new ModuleVersionInfo(fileVersion, productVersion, desc, company, productName, comments);
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
					builder.Clear();
					builder.AppendFormat($"0x{offset:x}");					
					return;
			}
		}

		public void GetNameByInlineContext(ulong offset, uint inlineContext, ref StringBuilder builder, out ulong displacement)
		{
			uint nameSize;
			var res = _symbols.GetNameByInlineContextWide(offset, inlineContext, builder, builder.Capacity, out nameSize, out displacement);
			switch (res)
			{
				case 0: // S_OK					
					return;
				case 1: // S_FALSE					
					builder = new StringBuilder((int)nameSize);
					GetSymbolNameByOffset(offset, ref builder, out displacement);
					return;
				default:
					builder.Clear();
					builder.AppendFormat($"0x{offset:x}");					
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

		public uint GetTypeId(string name)
		{
			uint typeId;
			var res = _symbols.GetTypeId(0, name, out typeId);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbols.GetTypeId");
			}
			return typeId;
		}

		public uint GetTypeSize(uint typeId)
		{
			uint size;
			var res = _symbols.GetTypeSize(0, typeId, out size);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbols.GetTypeSize");
			}
			return size;
		}

		public void SetScopeFrameByIndex(ushort frameNumber)
		{
			int res = _symbols.SetScopeFrameByIndex(frameNumber);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbols2.SetScopeFrameByIndex");
			}
		}

		public ScopeSymbolGroup GetScopeSymbolGroup()
		{
			IDebugSymbolGroup2 group;
			int res = _symbols.GetScopeSymbolGroup2(DEBUG_SCOPE_GROUP.ALL, null, out group);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbols2.GetScopeSymbolGroup2");
			}
			return new ScopeSymbolGroup(group);
		}

		public DEBUG_MODULE_PARAMETERS GetModuleParameters(uint moduleIndex)
		{
			DEBUG_MODULE_PARAMETERS[] moduleParam = new DEBUG_MODULE_PARAMETERS[1];
			int res = _symbols.GetModuleParameters(1, null, moduleIndex, moduleParam);
			if(res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbols.GetModuleParameters");
			}
			return moduleParam[0];

		}

		public void SetSymbolPath(string symbolPath)
		{
			var res = _symbols.SetSymbolPathWide(symbolPath);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbols.SetSymbolPathWide");
			}
		}

		public int Reload(string moduleName)
		{
			var res = _symbols.ReloadWide(moduleName);
			return res;
		}

		public void Dispose()
		{
			var res = Marshal.FinalReleaseComObject(_symbols);
			Debug.Assert(res == 0);
		}
	}

	
	internal class ScopeSymbolGroup
	{
		private readonly IDebugSymbolGroup2 _group;

		public ScopeSymbolGroup(IDebugSymbolGroup2 group)
		{
			_group = group;						
		}

		public SymbolGroup GetSymbolGroup(int levels)
		{
			var count = GetSymbolCount();
			uint start = 0;
			var parameters = GetSymbolParameters(start, count);
			start = count;
			var entries = new DEBUG_SYMBOL_ENTRY[count];
			var symbolGroup = new SymbolGroup();

						
			for (uint j = 0; j < GetSymbolCount(); j++)
			{			
					
				var entry = GetSymbolEntryInformation(j);
				var parameter = parameters[j];
				entries[j] = entry;
								
				var nameSize = entry.NameSize;
				var name = GetSymbolName(j, nameSize == 0 ? 50 : nameSize);
				var typeName = GetSymbolTypeName(j);
				var strValue = GetSymbolStringValue(j);
				var size = GetSymbolSize(j);
				
				var sym = new SymbolValue(name, typeName, strValue, size, entry, parameters[j]);				
				symbolGroup.AddSymbol(sym);
				if (sym.ExpansionDepth < levels - 1)
				{
					ExpandSymbol(j);
				}
			}
			
			return symbolGroup;
		}

		bool ExpandSymbol(uint index)
		{
			int res = _group.ExpandSymbol(index, true);
			switch (res)
			{
				case 0:
					ThrowDebuggerException(res, "IDebugSymbolGroup2.ExpandSymbol");
					return false;
				case 1:
					return false;
				default:
					return true;
			}
		}

		private DEBUG_SYMBOL_PARAMETERS[] GetSymbolParameters(uint start, uint count)
		{
			DEBUG_SYMBOL_PARAMETERS[] parameters = new DEBUG_SYMBOL_PARAMETERS[count];
			if (count > 0) {
				int res = _group.GetSymbolParameters(start, count, parameters);
				if (res != 0)
				{
					ThrowDebuggerException(res, "IDebugSymbolGroups2.GetSymbolParameters");
				}
			}
			return parameters;
		}

		private uint GetSymbolSize(uint index)
		{
			uint size;
			int res = _group.GetSymbolSize(index, out size);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbolGroup.GetSymbolSize");
			}

			return size;
		}

		private string GetSymbolName(uint index, uint sizeHint)
		{
			var sb = new StringBuilder((int) sizeHint);
			uint size;
			int res = _group.GetSymbolNameWide(index, sb, sb.Capacity, out size);
			if (res == 1)
			{
				sb.Capacity = (int) size;
				res = _group.GetSymbolNameWide(index, sb, sb.Capacity, out size);
			}
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbolGroup2.GetSymbolNameWide");
			}
			return sb.ToString();
		}

		private string GetSymbolTypeName(uint index)
		{
			var sb = new StringBuilder(50);
			uint size;
			int res = _group.GetSymbolTypeNameWide(index, sb, sb.Capacity, out size);
			if (res == 1)
			{
				sb.Capacity = (int)size;
				res = _group.GetSymbolTypeNameWide(index, sb, sb.Capacity, out size);
			}
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbolGroup2.GetSymbolTypeNameWide");
			}
			return sb.ToString();
		}

		private string GetSymbolStringValue(uint index)
		{
			var sb = new StringBuilder(50);
			uint size;
			int res = _group.GetSymbolValueTextWide(index, sb, sb.Capacity, out size);
			if (res == 1)
			{
				sb.Capacity = (int)size;
				res = _group.GetSymbolValueTextWide(index, sb, sb.Capacity, out size);
			}
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbolGroup2.GetSymbolTypeNameWide");
			}
			return sb.ToString();
		}

		private DEBUG_SYMBOL_ENTRY GetSymbolEntryInformation(uint index)
		{
			DEBUG_SYMBOL_ENTRY entry;
			int res = _group.GetSymbolEntryInformation(index, out entry);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSymbolGroups2.GetSymbolEntryInformation");
			}
			return entry;
		}


		private uint GetSymbolCount()
		{
			uint numSym;
			int res = _group.GetNumberSymbols(out numSym);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugsymbolGroup2.GetNumberSymbols");
			}
			return numSym;
		}
	}

	[DebuggerDisplay("{Name} ({TypeName}) = {StrValue}")]
	public class SymbolValue
	{
		public string Name { get; }
		public string TypeName { get;  }
		public string StrValue { get;  }
		public uint Size { get; }
		public DEBUG_SYMBOL_ENTRY DebugSymbolEntry { get; }
		public DEBUG_SYMBOL_PARAMETERS DebugSymbolParameters { get; }
		public uint? ParentSymbolId => DebugSymbolParameters.ParentSymbol == uint.MaxValue ? (uint?) null : DebugSymbolParameters.ParentSymbol;
		public uint TypeId => DebugSymbolParameters.TypeId;

		public int ExpansionDepth => (int)DebugSymbolParameters.Flags & (int)DEBUG_SYMBOL.EXPANSION_LEVEL_MASK;

		public DEBUG_SYMBOL DebugSymbol => (DEBUG_SYMBOL) ((int)DebugSymbolParameters.Flags & ~(int)DEBUG_SYMBOL.EXPANSION_LEVEL_MASK);

		public SymbolValue(string name, string typeName, string strValue, uint size, DEBUG_SYMBOL_ENTRY debugSymbolEntry, DEBUG_SYMBOL_PARAMETERS debugSymbolParameters)
		{
			Name = name;
			TypeName = typeName;
			StrValue = strValue;
			Size = size;
			DebugSymbolEntry = debugSymbolEntry;
			DebugSymbolParameters = debugSymbolParameters;
		}

	}

	public class SymbolGroup
	{
		readonly List<SymbolValue> _symbols = new List<SymbolValue>();
		public IList<SymbolValue> Symbols => _symbols.AsReadOnly();

		public void AddSymbol(string name, string typeName, string strValue, uint size, DEBUG_SYMBOL_ENTRY debugSymbolEntry, DEBUG_SYMBOL_PARAMETERS debugSymbolParameters)
		{
			_symbols.Add(new SymbolValue(name, typeName, strValue, size, debugSymbolEntry, debugSymbolParameters));				
		}

		public void AddSymbol(SymbolValue symbol)
		{
			_symbols.Add(symbol);
		}
	}
}