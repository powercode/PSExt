#pragma once
#include <windows.h>
#include "nowarn/dbghelp.h"
#include <vector>


using ModuleVector = std::vector<_IMAGEHLP_MODULEW64>;
ModuleVector GetNativeModules();