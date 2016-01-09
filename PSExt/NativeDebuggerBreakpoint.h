#pragma once
#include <vector>
#include "engextcpp.hpp"
#include <string>


using DString = std::wstring;

struct NativeBreakpointData{
	DEBUG_BREAKPOINT_PARAMETERS Parameters;
	DString Command;
	DString OffsetExpression;
public:
	NativeBreakpointData()
	{
		memset(&Parameters, 0, sizeof(DEBUG_BREAKPOINT_PARAMETERS));
	}
	NativeBreakpointData(const DEBUG_BREAKPOINT_PARAMETERS& parameters, DString&& command, DString&& offsetExpression)
		: Parameters(parameters)
		, Command(command)
		, OffsetExpression(offsetExpression)
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
				auto command = GetCommand(pbp, bp.CommandSize);
				auto offsetExpression = GetOffsetExpression(pbp, bp.OffsetExpressionSize);
				retval.emplace_back(bp, move(command), move(offsetExpression));
			}
		}
		return retval;
	}

	static DString GetCommand(PDEBUG_BREAKPOINT2 bp, ULONG commandSize){		
		DString command(commandSize, 'L\0');
		if (commandSize != 0){
			auto res = bp->GetCommandWide(const_cast<DString::value_type*>(command.data()), commandSize, nullptr);
			if (FAILED(res)){
				throw std::runtime_error("Failed to get breakpoint command");
			}
		}
		return command;
	}

	static DString GetCommand(PDEBUG_BREAKPOINT2 bp){
		ULONG commandSize;
		auto res = bp->GetCommandWide(nullptr, 0, &commandSize);
		if (FAILED(res)){
			throw std::runtime_error("Failed to get breakpoint command size");
		}
		return GetCommand(bp, commandSize);
	}

	static DString GetOffsetExpression(PDEBUG_BREAKPOINT2 bp, ULONG expressionSize){
		DString expression(expressionSize, L'\0');
		if (expressionSize != 0){
			auto res = bp->GetOffsetExpressionWide(const_cast<DString::value_type*>(expression.data()), expressionSize, nullptr);
			if (FAILED(res)){
				throw std::runtime_error("Faile to get offset expression");
			}
		}
		return expression;
	}

	static DString GetOffsetExpression(PDEBUG_BREAKPOINT2 bp){
		ULONG expressionSize;
		auto res = bp->GetOffsetExpressionWide(nullptr, 0, &expressionSize);
		if (FAILED(res)){
			throw std::runtime_error("Faile to get offset expression size");
		}
		return GetOffsetExpression(bp, expressionSize);
	}
};