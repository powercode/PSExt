using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Diagnostics.Runtime.Interop;

using static PSExt.ExceptionHelper;

namespace PSExt
{
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
			var ids = new List<uint>();
			uint bpid;
			if (data.Expression != null)
			{
				
				var pattern = data.Expression;
				IList<SymbolSearchResult> matches = Symbols.GetMatchingSymbols(pattern);
				if (matches.Count == 0)
				{
					// do nothing
				}
				else {
					foreach (var match in matches)
					{
						bpid = BreakpointManager.AddBreakpointOffset(match.Offset, data);
						ids.Add(bpid);
					}
				}
			}
			else {
				bpid = BreakpointManager.AddBreakpointOffset(data.Offset, data);
				ids.Add(bpid);

			}
			var bps = BreakpointManager.GetBreakpoints();

			return bps.Where(bp => ids.Contains(bp.Id)).ToList();
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
					ThrowRemote(status, $"Unable to get module #{i}");
				}
												
				var buffer = new byte[Marshal.SizeOf<IMAGEHLP_MODULEW64>()];
				var sizeAsBytes = BitConverter.GetBytes(buffer.Length);
				Array.Copy(sizeAsBytes, buffer,sizeAsBytes.Length);
				int infoSize;				
				var bufferSize = buffer.Length;
				if ((status = _advanced2.GetSymbolInformation(DEBUG_SYMINFO.IMAGEHLP_MODULEW64, moduleBase, 0, buffer, bufferSize, out infoSize, null, 0, IntPtr.Zero)) != 0)
				{
					ThrowRemote(status, "Unable to retrieve module info");
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

		private void ThrowRemote(int status, string message)
		{
			throw new Win32Exception(status, message);
		}

		private IList<DEBUG_STACK_FRAME_EX> GetNativeStack()
		{
			var frames = new DEBUG_STACK_FRAME_EX[100];
			uint filled;			
			int status;
						
			if ((status = _control5.GetStackTraceEx(0, 0, 0, frames, frames.Length, out filled)) != 0)
			{
				ThrowRemote(status, "Unable to get callstack.");
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
			readonly StringBuilder _builder = new StringBuilder(1000);
			public int Output(DEBUG_OUTPUT mask, string text)
			{
				_builder.Append(text);
				return 0;
			}
		
			public string Text => _builder.ToString();
		}
	}

	internal class BreakpointManager
	{
		private readonly IDebugControl5 _control5;
		private readonly StringBuilder _builder = new StringBuilder(256);

		public BreakpointManager(IDebugClient6 client)
		{
			_control5 = (IDebugControl5) client;
		}

		private uint GetBreakpointCount()
		{
			uint number;
			var res = _control5.GetNumberBreakpoints(out number);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugControl5::GetNumberBreakpoints");
			}
			return number;
		}

		public uint AddBreakpointOffset(ulong offset, BreakpointData data)
		{
			IDebugBreakpoint2 pBp;
			var res = _control5.AddBreakpoint2(
				(DEBUG_BREAKPOINT_TYPE) data.BreakType,
				data.Id,
				out pBp);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugControl5.AddBreakpoint2");
			}
			pBp.SetOffset(offset);
			{				
				pBp.SetCommandWide(data.Command);				
			}
			if (data.PassCount != 0)
			{
				pBp.SetPassCount(data.PassCount);
			}
			if (data.MatchThread != 0)
			{
				pBp.SetMatchThreadId(data.MatchThread);
			}
			if (data.MatchThread != 0)
			{
				pBp.SetMatchThreadId(data.MatchThread);
			}
			if (data.BreakType == BreakType.Data)
			{
				pBp.SetDataParameters(data.DataSize, (DEBUG_BREAKPOINT_ACCESS_TYPE) data.DataBreakpointKind);
			}
			if (data.Flags != BreakpointFlags.None)
			{
				pBp.AddFlags((DEBUG_BREAKPOINT_FLAG) data.Flags);
			}
			uint newId;
			res = pBp.GetId( out newId);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugBreakpoint2.GetId");
			}
			return newId;
		}

		public IList<BreakpointData> GetBreakpoints()
		{
			const uint DEBUG_ANY_ID = 0xffffffff;
			var count = GetBreakpointCount();
			var retval = new List<BreakpointData>((int)count);
			if (count != 0)
			{
				var bps = new DEBUG_BREAKPOINT_PARAMETERS[count];
				var res = _control5.GetBreakpointParameters(count, null, 0, bps);
				if (res != 0)
				{
					ThrowDebuggerException(res, "_control5.GetBreakpointParameters");
				}

				
				foreach (var bp in bps.Where(bp => bp.Id != DEBUG_ANY_ID))
				{
					IDebugBreakpoint2 bp2;
					res = _control5.GetBreakpointById2(bp.Id, out bp2);
					if (res != 0)
					{
						continue;
					}
					var command = GetCommand(bp2, bp.CommandSize);
					var offsetExpression = GetOffsetExpression(bp2, bp.OffsetExpressionSize);
					ulong offset;
					bp2.GetOffset(out offset);

					var p = bp;
					var k = (BreakType)p.BreakType;
					var flags = (BreakpointFlags)p.Flags;
					var dk = (DataBreakpointAccessTypes)p.DataAccessType;
					
					var bpd = new BreakpointData(p.Offset, k, flags, dk, p.DataSize, p.ProcType, p.MatchThread,
						p.Id, p.PassCount, p.CurrentPassCount, command, offsetExpression);

					retval.Add(bpd);
				}
			}
			return retval;
		}

		string GetCommand(IDebugBreakpoint2 bp, uint commandSize)
		{
			_builder.EnsureCapacity((int) commandSize);				
			if (commandSize != 0)
			{
				uint size;
				var res = bp.GetCommandWide(_builder, _builder.Capacity, out size);
				if (res!=0)
				{
					ThrowDebuggerException(res, "IDebugBreakpoint2.GetCommandWide");
				}
			}
			return _builder.ToString();
		}

		string GetCommand(IDebugBreakpoint2 bp)
		{
			uint commandSize;
			var res = bp.GetCommandWide(null, 0, out commandSize);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugBreakpoint2.GetCommandWide");
			}
			return GetCommand(bp, commandSize);
		}

		string GetOffsetExpression(IDebugBreakpoint2 bp, uint expressionSize)
		{			
			if (expressionSize != 0)
			{
				_builder.EnsureCapacity((int) expressionSize);
				var res = bp.GetOffsetExpressionWide(_builder,_builder.Capacity, out expressionSize);
				if (res != 0)
				{
					ThrowDebuggerException(res, "IDebugBreakpoint2.GetOffsetExpressionWide");
				}
			}
			return _builder.ToString();
		}

		string GetOffsetExpression(IDebugBreakpoint2 bp)
		{
			uint expressionSize;
			var res = bp.GetOffsetExpressionWide(null, 0, out expressionSize);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugBreakpoint2.GetOffsetExpressionWide");
			}
			return GetOffsetExpression(bp, expressionSize);
		}
	}

	internal class ExceptionHelper
	{
		public static void ThrowDebuggerException(int statusCode, string failingMethod)
		{
			throw new DebuggerException(statusCode, failingMethod);
		}
	}

	internal class DebuggerException : Win32Exception
	{
		public string FailingMethod { get; set; }
		public int StatusCode { get; set; }

		public DebuggerException(int statusCode, string failingMethod) : base(statusCode)
		{
			FailingMethod = failingMethod;
		}
	}

	class SymbolSearchResult
	{
		public UInt64 Offset;
		public String Name;

		public SymbolSearchResult(ulong offset, string name)
		{
			Offset = offset;
			Name = name;
		}
	}

	class Symbols
	{
		private readonly IDebugSymbols3 _symbols;

		public Symbols(IDebugSymbols3 symbols)
		{
			_symbols = symbols;
		}

		public IList<SymbolSearchResult> GetMatchingSymbols(string pattern)
		{
			var retval = new List<SymbolSearchResult>(100);
			using (var searcher = new SymbolsSearch(_symbols, pattern)) {			
				SymbolSearchResult res;
				while (searcher.GetNextMatch(out res))
				{
					retval.Add(res);
				}
				return retval;
			}
		}

		public void GetSymbolNameByOffset(ulong offset, ref StringBuilder builder, out ulong displacement)
		{
			uint nameSize;
			var res = _symbols.GetNameByOffsetWide(offset, builder, builder.Capacity, out nameSize, out displacement);
			switch (res)
			{
				case 0: // S_OK					
					return;
				case 1: // S_FALSE					
					builder = new StringBuilder((int)nameSize);
					GetSymbolNameByOffset(offset, ref builder, out displacement);
					return;
				default:
					ThrowDebuggerException(res, "IDebugSymbols3.GetNameByOffsetWide");
					return;
			}
		}

		class SymbolsSearch : IDisposable
		{
			private readonly IDebugSymbols3 _symbols;
			private readonly UInt64 _searchHandle;
			private readonly StringBuilder _builder = new StringBuilder(256);

			const int E_NOINTERFACE = unchecked ((int) 0x80004002);

			public SymbolsSearch(IDebugSymbols3 symbols, string pattern)
			{
				_symbols = symbols;
				_symbols.StartSymbolMatchWide(pattern, out _searchHandle);
			}

			public bool GetNextMatch(out SymbolSearchResult nextMatch)
			{
				ulong offset;
				uint matchSize;
				int res = _symbols.GetNextSymbolMatchWide(_searchHandle, _builder, _builder.Capacity, out matchSize, out offset);
				switch (res)
				{
					case 0: // S_OK
						nextMatch = new SymbolSearchResult(offset, _builder.ToString());
						return true;
					case 1: // S_FALSE
						_builder.EnsureCapacity((int) matchSize);
						return GetNextMatch(out nextMatch);
					case E_NOINTERFACE:
						nextMatch = null;
						return false;
					default:
						nextMatch = null;
						return false;
				}				
			}

			public void Dispose()
			{
				_symbols.EndSymbolMatch(_searchHandle);
			}
		}
	}
}