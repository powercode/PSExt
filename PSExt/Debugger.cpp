#include "Debugger.h"
#include "NativeModule.h"
#include "Symbols.h"
#include "NativeDebuggerBreakpoint.h"
#include "NativeCallstack.h"
#include <msclr/marshal.h>
#include <msclr/marshal_cppstd.h>


using namespace System;
using namespace System::Collections::Generic;
using namespace PSExt;
using namespace msclr::interop;

namespace PSExt {
	namespace Native {

		namespace helpers
		{

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
		
			BreakpointData^ ToBreakpoint(const NativeBreakpointData& data) {
				auto& p = data.Parameters;
				auto k = (BreakType)p.BreakType;
				auto flags = (BreakpointFlags)p.Flags;
				auto dk = (DataBreakpointAccessTypes)p.DataAccessType;

				auto cmd = marshal_as<String^>(data.Command);
				auto expr = marshal_as<String^>(data.OffsetExpression);
				return gcnew BreakpointData(p.Offset, k, flags, dk, p.DataSize, p.ProcType, p.MatchThread,
					p.Id, p.PassCount, p.CurrentPassCount, cmd, expr);
			}


			ULONG AddBreakpointOffset(ULONG64 offset, BreakpointData^ data) {
				PDEBUG_BREAKPOINT2 pBp;
				auto res = g_Ext->m_Control4->AddBreakpoint2(
					static_cast<ULONG>(data->BreakType),
					data->Id,
					&pBp);
				if (FAILED(res)) {
					throw std::runtime_error("failed to set breakpoint");
				}
				pBp->SetOffset(offset);
				{
					marshal_context context;
					if (data->Command != nullptr) {
						pBp->SetCommandWide(context.marshal_as<PCWSTR>(data->Command));
					}
				}
				if (data->PassCount != 0) {
					pBp->SetPassCount(data->PassCount);
				}
				if (data->MatchThread != 0) {
					pBp->SetMatchThreadId(data->MatchThread);
				}
				if (data->MatchThread != 0) {
					pBp->SetMatchThreadId(data->MatchThread);
				}
				if (data->BreakType == BreakType::Data) {
					pBp->SetDataParameters(data->DataSize, static_cast<ULONG>(data->DataBreakpointKind));
				}
				if (data->Flags != BreakpointFlags::None) {
					pBp->AddFlags(static_cast<ULONG>(data->Flags));
				}
				ULONG newId;
				if (FAILED(pBp->GetId(&newId))) {
					throw std::runtime_error("failed to get breakpoint id for new breakpoint");
				}
				return newId;
			}
		
			StackFrame^ ToStackFrame(const DEBUG_STACK_FRAME_EX& frame, PSExt::Callstack^ stack)
			{
				UNREFERENCED_PARAMETER(frame);
				ULONG64 displacement = 0;
				auto name = Symbols::GetNameByOffset(frame.InstructionOffset, OUT displacement);
				String^ nameStr = marshal_as<String^>(name.c_str());
				return gcnew StackFrame(frame.ReturnOffset, frame.InstructionOffset, frame.FrameOffset, USHORT(frame.FrameNumber), nameStr, displacement, stack);
			}
		}



		String^ Debugger::ExecuteCommand(String^ command)
		{
			auto cmd = _marshal_context.marshal_as<PCSTR>(command);
			ExtCaptureOutputW outputCapture;
			outputCapture.Execute(cmd);
			auto res = outputCapture.GetTextNonNull();
			auto mres = msclr::interop::marshal_as<String^>(res);
			return mres;
		}

		String^ Debugger::ReadLine() {
			wchar_t buf[256];
			ULONG inputSize = 0;
			g_ExtInstancePtr->m_Control4->InputWide(buf, 256, &inputSize);
			return msclr::interop::marshal_as<String^>(buf);
		}

		void Debugger::Write(System::String^ output) {
			auto str = _marshal_context.marshal_as<PCWSTR>(output);
			g_ExtInstancePtr->Out(str);
		}

		IList<BreakpointData^>^ Debugger::GetBreakpoints() {
			auto bps = NativeDebuggerBreakpoint::GetBreakpoints();
			List<BreakpointData^>^ res = gcnew List<BreakpointData^>((int)bps.size());
			for (const auto& nbp : bps) {
				res->Add(helpers::ToBreakpoint(nbp));
			}
			return res;
		}

		IList<BreakpointData^>^ Debugger::AddBreakpoints(BreakpointData^ data) {
			auto retVal = gcnew List<BreakpointData^>();
			auto ids = gcnew List<UInt32>();
			ULONG bpid;
			if (data->Expression != nullptr) {
				marshal_context context;
				auto pattern = context.marshal_as<PCWSTR>(data->Expression);
				auto matches = Symbols::GetMatchingSymbols(pattern);
				if (matches.size() == 0) {

				}
				else {
					for (auto& match : matches) {
						bpid = helpers::AddBreakpointOffset(match.Offset, data);
						ids->Add(bpid);
					}
				}
			}
			else {
				bpid = helpers::AddBreakpointOffset(data->Offset, data);
				ids->Add(bpid);

			}
			auto bps = NativeDebuggerBreakpoint::GetBreakpoints();
			for (auto& bp : bps) {
				if (ids->Contains(bp.Parameters.Id)) {
					retVal->Add(helpers::ToBreakpoint(bp));
				}
			}

			return retVal;
		}

		IList<ModuleData^>^ Debugger::GetModules() {
			auto mods = GetNativeModules();
			List<ModuleData^>^ res = gcnew List<ModuleData^>((int)mods.size());
			for (auto& mod : mods)
			{
				res->Add(helpers::ToModule(mod));
			}
			return res;
		}

		PSExt::Callstack^ Debugger::GetCallstack() {
			auto stackFrames = GetNativeStack();
			List<StackFrame^>^ res = gcnew List<StackFrame^>((int)stackFrames.size());
			auto retVal = gcnew PSExt::Callstack(res);
			for (auto& frame : stackFrames)
			{
				res->Add(helpers::ToStackFrame(frame, retVal));
			}
			return retVal;
		}
	}
}