#pragma once
#include <vector>
#include <string>
#include "engextcpp.hpp"
#include "nowarn/dbghelp.h"


ref class Callstack {
public:
	static PSExt::Callstack^ GetCallstacks();

};
