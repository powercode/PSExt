#pragma once
#include <vector>
#include "engextcpp.hpp"
#include <string>


using DString = std::wstring;

struct NativeBreakpointData{
	DEBUG_BREAKPOINT_PARAMETERS _parameters;
	DString _command;
	DString _offsetExpression;
public:
	NativeBreakpointData(const DEBUG_BREAKPOINT_PARAMETERS& parameters, DString&& command, DString&& offsetExpression)
		: _parameters(parameters)
		, _command(command)
		, _offsetExpression(offsetExpression)
	{
	}
};

using Breakpoints = std::vector < NativeBreakpointData >;



class NativeDebuggerBreakpoint{
public:
	static LONG GetNumberBreakpoints(){
		ULONG breakpointsCount;
		if (SUCCEEDED(g_Ext->m_Control->GetNumberBreakpoints(&breakpointsCount))){
			return breakpointsCount;
		}
		throw std::runtime_error("failed to get breakpoint count");
	}
	static Breakpoints GetBreakpoints(){
		Breakpoints retval;
		auto count = GetNumberBreakpoints();
		if (count != 0){
			std::vector<DEBUG_BREAKPOINT_PARAMETERS> bps((size_t)count);
			auto res = g_Ext->m_Control->GetBreakpointParameters(count, NULL, 0, &bps[0]);
			if (FAILED(res)){
				throw std::runtime_error("failed to get breakpoint parameters");
			}

			retval.reserve(count);
			for (auto& bp : bps){
				if (bp.Id == DEBUG_ANY_ID){
					continue;
				}
				PDEBUG_BREAKPOINT2 pbp;
				res = g_Ext->m_Control4->GetBreakpointById2(bp.Id, &pbp);
				if (FAILED(res)){
					continue;
				}
				auto command = GetCommand(pbp);
				auto offsetExpression = GetOffsetExpression(pbp);
				retval.emplace_back(bp, move(command), move(offsetExpression));
			}
		}
		return retval;
	}

	static DString GetCommand(PDEBUG_BREAKPOINT2 bp){
		ULONG commandSize;
		auto res = bp->GetCommandWide(nullptr, 0, &commandSize);
		if (FAILED(res)){
			throw std::runtime_error("Failed to get breakpoint command size");
		}
		DString command;
		command.reserve(commandSize);
		res = bp->GetCommandWide(const_cast<DString::value_type*>(command.data()), commandSize, nullptr);
		if (FAILED(res)){
			throw std::runtime_error("Failed to get breakpoint command");
		}
		return command;
	}

	static DString GetOffsetExpression(PDEBUG_BREAKPOINT2 bp){
		ULONG expressionSize;
		auto res = bp->GetOffsetExpressionWide(nullptr, 0, &expressionSize);
		if (FAILED(res)){
			throw std::runtime_error("Faile to get offset expression size");
		}
		DString expression;
		expression.reserve(expressionSize);
		res = bp->GetOffsetExpressionWide(const_cast<DString::value_type*>(expression.data()), expressionSize, nullptr);
		if (FAILED(res)){
			throw std::runtime_error("Faile to get offset expression");
		}
		return expression;
	}
};