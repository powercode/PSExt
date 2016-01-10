using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Diagnostics.Runtime.Interop;

namespace PSExt
{
	// ReSharper disable SuspiciousTypeConversion.Global
	internal class Debugger : IDebugger
	{
		
		private readonly IDebugClient6 _client;
		private readonly IDebugControl5 _control5;
		private readonly IDebugAdvanced2 _advanced2;
		private BreakpointManager _breakpointManager;
		private Symbols _symbols;

		public BreakpointManager BreakpointManager => _breakpointManager ?? (_breakpointManager = new BreakpointManager(_client));
		public Symbols Symbols => _symbols ?? (_symbols = new Symbols((IDebugSymbols3)_client));


		public Debugger(IDebugClient client)
		{
			_client = (IDebugClient6) client;
			
			_control5 = (IDebugControl5) client;
			_advanced2 = (IDebugAdvanced2) client;			
		}

		public string ExecuteCommand(string command)
		{
			IDebugOutputCallbacksWide oldCallbacks;			
			_client.GetOutputCallbacksWide(out oldCallbacks);
			var output = new DebugOutput();
			try
			{
				_client.SetOutputCallbacksWide(output);
				_control5.Execute(DEBUG_OUTCTL.THIS_CLIENT, command, DEBUG_EXECUTE.DEFAULT);
				return output.Text;
			}
			finally
			{
				_client.SetOutputCallbacksWide(oldCallbacks);
			}
		}

		public string ReadLine()
		{
			var builder = new StringBuilder(256);
			uint inputSize;
			_control5.InputWide(builder, builder.Length, out inputSize);
			return builder.ToString();
		}

		public void Write(string value)
		{
			_control5.Output(DEBUG_OUTPUT.NORMAL, value);			
		}

		public IList<BreakpointData> GetBreakpoints()
		{
			return BreakpointManager.GetBreakpoints();			
		}

		public IList<BreakpointData> AddBreakpoints(BreakpointData data)
		{
			return BreakpointManager.AddBreakpoints(data);			
		}

		public IList<ModuleData> GetModules()
		{
			var modules = new List<ModuleData>(50);
			   
			UInt32 loaded;
			UInt32 unloaded;			
			var symbols = (IDebugSymbols) _client;
			symbols.GetNumberModules(out loaded, out unloaded);
			
			for (UInt32 i = 0; i < loaded; ++i)
			{
				ulong moduleBase;
				int status;
				if ((status = symbols.GetModuleByIndex(i, out moduleBase)) != 0)
				{
					ExceptionHelper.ThrowDebuggerException(status, "IDebugSymbols.GetModuleByIndex");
				}
												
				var buffer = new byte[Marshal.SizeOf<IMAGEHLP_MODULEW64>()];
				var sizeAsBytes = BitConverter.GetBytes(buffer.Length);
				Array.Copy(sizeAsBytes, buffer,sizeAsBytes.Length);
				int infoSize;				
				var bufferSize = buffer.Length;
				if ((status = _advanced2.GetSymbolInformation(DEBUG_SYMINFO.IMAGEHLP_MODULEW64, moduleBase, 0, buffer, bufferSize, out infoSize, null, 0, IntPtr.Zero)) != 0)
				{
					ExceptionHelper.ThrowDebuggerException(status, "IDebugSymbols.GetSymbolInformation");
				}
				var md = ToModuleData(buffer);
				modules.Add(md);
			}

			return modules;
		}

		private unsafe ModuleData ToModuleData(byte[] moduleInfoBuffer)
		{
			fixed (byte* buf = moduleInfoBuffer)
			{			
				IMAGEHLP_MODULEW64 mi = Marshal.PtrToStructure<IMAGEHLP_MODULEW64>(new IntPtr(buf));
				return new ModuleData(mi.ModuleName, mi.ImageName, mi.LoadedImageName, mi.LoadedPdbName, mi.BaseOfImage, mi.ImageSize,
					 mi.TimeDateStamp, mi.CheckSum, mi.NumSyms, (uint)mi.SymType, mi.PdbSig70, mi.PdbAge, mi.PdbUnmatched, mi.LineNumbers, mi.GlobalSymbols, 
					 mi.TypeInfo, mi.SourceIndexed, mi.Publics, mi.MachineType);
			}
		}


		private IList<DEBUG_STACK_FRAME_EX> GetNativeStack()
		{
			var frames = new DEBUG_STACK_FRAME_EX[100];
			uint filled;			
			int status;
						
			if ((status = _control5.GetStackTraceEx(0, 0, 0, frames, frames.Length, out filled)) != 0)
			{
				ExceptionHelper.ThrowDebuggerException(status, "IDebugControl5.GetStackTraceEx");
			}
			return new List<DEBUG_STACK_FRAME_EX>(frames.Take((int)filled));
		}

		public DebugThread GetCallstack()
		{
			var nativeFrames = GetNativeStack();
			var stackFrames = new List<StackFrame>(nativeFrames.Count);
			var retVal = new DebugThread(stackFrames);
			stackFrames.AddRange(nativeFrames.Select(frame => ToStackFrame(frame, retVal)));
			return retVal;
		}

		private StackFrame ToStackFrame(DEBUG_STACK_FRAME_EX frame, DebugThread thread)
		{
			ulong displacement;
			var builder = new StringBuilder(256);
			Symbols.GetSymbolNameByOffset(frame.InstructionOffset, ref builder, out displacement);
			var name = builder.ToString();
			return new StackFrame(frame.ReturnOffset, frame.InstructionOffset, frame.FrameOffset, (ushort)frame.FrameNumber, name, displacement, thread);
		}				

		class DebugOutput : IDebugOutputCallbacksWide
		{
			private readonly StringBuilder _builder = new StringBuilder(1000);
			
			public int Output(DEBUG_OUTPUT mask, string text)
			{
				_builder.Append(text);
				return 0;
			}
		
			public string Text => _builder.ToString();
		}
	}
}