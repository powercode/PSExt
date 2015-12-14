#pragma once
#include "nowarn/dbgeng.h"
#include <string>
#include <vector>

using DString = std::wstring;



struct SymbolSearchResult{
	ULONG64 Offset;
	DString Name;
	SymbolSearchResult();
};

using SymbolSearchResults = std::vector < SymbolSearchResult > ;

class Symbols final {
	static bool GetNameByOffset(ULONG64 offset, OUT DString name);
public:
	static SymbolSearchResults GetMatchingSymbols(PCWSTR pattern);
	static DString GetNameByOffset(ULONG64 offset);
};

class SymbolsSearch final{
	ULONG64 _searchHandle;
public:	
	explicit SymbolsSearch(PCWSTR pattern);
	bool GetNextMatch(OUT SymbolSearchResult& nextMatch) const;
	~SymbolsSearch();
};
