#include "Module.h"
#include "engextcpp.hpp"
#include <msclr/marshal.h>
#include <vector>



using namespace std;

using namespace System;
using namespace PSExt;
using namespace System::Collections::Generic;
using namespace msclr::interop;


Guid FromGUID(const GUID& guid) {
	return Guid(guid.Data1, guid.Data2, guid.Data3,
		guid.Data4[0], guid.Data4[1],
		guid.Data4[2], guid.Data4[3],
		guid.Data4[4], guid.Data4[5],
		guid.Data4[6], guid.Data4[7]);
}

ModuleData^ ToModule(const _IMAGEHLP_MODULEW64& m) {
	
	auto moduleName = marshal_as<String^>(m.ModuleName);
	auto imageName = marshal_as<String^>(m.ImageName);
	auto loadedImageName = marshal_as<String^>(m.LoadedImageName);
	auto loadedPdbName = marshal_as<String^>(m.LoadedPdbName);	
	
	return gcnew ModuleData(moduleName, imageName, loadedImageName, loadedPdbName, m.BaseOfImage, m.ImageSize, m.TimeDateStamp, 
		m.CheckSum, m.NumSyms, m.SymType, FromGUID(m.PdbSig70), m.PdbAge, m.PdbUnmatched != 0,
		 m.LineNumbers != 0, m.GlobalSymbols != 0, m.TypeInfo != 0, m.SourceIndexed != 0, m.Publics != 0, m.MachineType);
}


ModuleVector GetNativeModules()
{
	ModuleVector retVal;
	retVal.reserve(40);

	ULONG loaded;
	ULONG unloaded;
	auto& sym = g_Ext->m_Symbols;
	HRESULT status;
	if ((status = sym->GetNumberModules(&loaded, &unloaded)) != S_OK)
	{
		g_Ext->ThrowRemote(status, "Unable to get number of modules");
	}
	ULONG64 moduleBase;
	for (ULONG i = 0; i < loaded; ++i)
	{
		if ((status = sym->GetModuleByIndex(i, &moduleBase)) != S_OK)
		{
			g_Ext->ThrowRemote(status, "Unable to get module #%d", i);
		}
		_IMAGEHLP_MODULEW64 moduleInfo;
		g_Ext->GetModuleImagehlpInfo(moduleBase, &moduleInfo);
		retVal.push_back(moduleInfo);
	}

	return retVal;
}


IList<ModuleData^>^ Modules::GetModules()
{	
	auto mods = GetNativeModules();
	List<ModuleData^>^ res = gcnew List<ModuleData^>((int)mods.size());
	for (auto& mod : mods)
	{
		res->Add(ToModule(mod));
	}
	return res;
}
