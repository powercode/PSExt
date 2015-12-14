#include "Symbols.h"
#include "engextcpp.hpp"


SymbolSearchResults Symbols::GetMatchingSymbols(PCWSTR pattern){
	SymbolSearchResults retval;
	SymbolsSearch searcher(pattern);
	SymbolSearchResult res;
	while (searcher.GetNextMatch(OUT res)){
		retval.push_back(res);
	}
	return retval;
}
bool Symbols::GetNameByOffset(ULONG64 offset, OUT DString name){
	auto buf = const_cast<PWSTR>(name.data());
	auto buflen = static_cast<ULONG>(name.size());
	ULONG nameSize;
	ULONG64 displacement ;
	auto res = g_Ext->m_Symbols3->GetNameByOffsetWide(offset, buf, buflen, &nameSize, &displacement);
	switch (res){
		case S_OK:
			name.resize(nameSize);
			return true;
		case S_FALSE:
			name.resize(nameSize);
			return GetNameByOffset(offset, name);
		default:
			return false;
	}
}
DString Symbols::GetNameByOffset(ULONG64 offset){
	DString name(80, L'\0');
	if (!GetNameByOffset(offset, name)){
		throw std::runtime_error("could not get name at offset");
	}
	return name;	
}

SymbolSearchResult::SymbolSearchResult() 
	: Offset(0)
	, Name(50, L'\0')
{
}

SymbolsSearch::SymbolsSearch(PCWSTR pattern){
	g_Ext->m_Symbols3->StartSymbolMatchWide(pattern, &_searchHandle);
}
SymbolsSearch::~SymbolsSearch(){
	g_Ext->m_Symbols3->EndSymbolMatch(_searchHandle);
}


bool SymbolsSearch::GetNextMatch(OUT SymbolSearchResult& nextMatch) const
{
	auto buf = const_cast<PWSTR>(nextMatch.Name.data());
	auto len = static_cast<ULONG>(nextMatch.Name.size());
	ULONG matchSize;
	ULONG64 offset;
	auto res = g_Ext->m_Symbols3->GetNextSymbolMatchWide(_searchHandle, buf, len, &matchSize, &offset);
	switch (res){
		case S_OK:
		{
			nextMatch.Name.resize(matchSize);
			nextMatch.Offset = offset;
			return true;
		}
		case S_FALSE:
		{
			nextMatch.Name.resize(matchSize);
			return GetNextMatch(OUT nextMatch);
		}
		case E_NOINTERFACE:{
			return false;
		}
		default:
			return false;
	}
}
