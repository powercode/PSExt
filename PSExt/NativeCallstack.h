#pragma once
#include "nowarn/dbgeng.h"
#include <vector>


using StackVector = std::vector<DEBUG_STACK_FRAME_EX>;
StackVector GetNativeStack();