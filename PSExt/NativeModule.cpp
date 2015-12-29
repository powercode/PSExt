#include "NativeModule.h"
#include "engextcpp.hpp"


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


