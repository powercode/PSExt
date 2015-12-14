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

class Symbols{
	static bool GetNameByOffset(ULONG64 offset, OUT DString name);
public:
	static SymbolSearchResults GetMatchingSymbols(PCWSTR pattern);
	static DString GetNameByOffset(ULONG64 offset);
};

class SymbolsSearch{
	ULONG64 _searchHandle;
public:	
	SymbolsSearch(PCWSTR pattern);
	bool GetNextMatch(OUT SymbolSearchResult& nextMatch);
	~SymbolsSearch();
};
