#pragma once
#using <PSExtCmdlets.dll>
#include "engextcpp.hpp"
#include <vector>
#include "nowarn/dbghelp.h"

ref class Modules {	
public:
	static System::Collections::Generic::IList<PSExt::ModuleData^>^ GetModules();	

};
using ModuleVector = std::vector<_IMAGEHLP_MODULEW64>;
ModuleVector GetNativeModules();