using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Microsoft.Diagnostics.Runtime.Interop;

using static PSExt.ErrorHelper;

namespace PSExt
{
	// ReSharper disable SuspiciousTypeConversion.Global
	internal class Debugger : IDebugger
	{

		private readonly IDebugClient6 _client;
		private readonly IDebugControl5 _control5;
		private readonly IDebugAdvanced2 _advanced2;
		private SystemObjects _systemObjects;
		private BreakpointManager _breakpointManager;
		private Symbols _symbols;
		private DebuggerThread _thread;

		public BreakpointManager BreakpointManager => _breakpointManager ?? (_breakpointManager = new BreakpointManager(_client));
		public Symbols Symbols => _symbols ?? (_symbols = new Symbols((IDebugSymbols3)_client));
		public SystemObjects SystemObjects => _systemObjects ?? (_systemObjects = new SystemObjects((IDebugSystemObjects2)_client));
		public DebuggerThread DebuggerThread => _thread ?? (_thread = new DebuggerThread(_client, Symbols));


		public Debugger(IDebugClient client)
		{
			_client = (IDebugClient6)client;
			_control5 = (IDebugControl5)client;
			_advanced2 = (IDebugAdvanced2)client;
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
			var symbols = (IDebugSymbols)_client;
			symbols.GetNumberModules(out loaded, out unloaded);

			for (UInt32 i = 0; i < loaded; ++i)
			{
				ulong moduleBase;
				int status;
				if ((status = symbols.GetModuleByIndex(i, out moduleBase)) != 0)
				{
					ThrowDebuggerException(status, "IDebugSymbols.GetModuleByIndex");
				}

				var buffer = new byte[Marshal.SizeOf<IMAGEHLP_MODULEW64>()];
				var sizeAsBytes = BitConverter.GetBytes(buffer.Length);
				Array.Copy(sizeAsBytes, buffer, sizeAsBytes.Length);
				int infoSize;
				var bufferSize = buffer.Length;
				if ((status = _advanced2.GetSymbolInformation(DEBUG_SYMINFO.IMAGEHLP_MODULEW64, moduleBase, 0, buffer, bufferSize, out infoSize, null, 0, IntPtr.Zero)) != 0)
				{
					ThrowDebuggerException(status, "IDebugSymbols.GetSymbolInformation");
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

		public IList<DebugThread> GetCallstack(bool all)
		{
			IList<DebugThread> retVal;
			if (all)
			{
				var threadInfos= SystemObjects.ThreadInfos;
				retVal = new List<DebugThread>(threadInfos.Length);
				foreach (var threadId in threadInfos)
				{
					retVal.Add(DebuggerThread.GetCallstack(threadId));
				}
			}
			else
			{
				var threadInfo = SystemObjects.CurrentThreadInfo;
				retVal = new List<DebugThread>(1) {DebuggerThread.GetCallstack(threadInfo)};
			}			
			return retVal;
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

	struct ThreadContext
	{
		public byte[] Context;

		public ThreadContext(byte[] context)
		{
			Context = context;
		}
	}

	class DebuggerThread
	{
		private const int ContextSizeX64 = 0x4d0;
		private readonly Symbols _symbols;
		private readonly IDebugControl5 _control5;
		private readonly IDebugAdvanced2 _advanced2;
		private readonly IDebugSystemObjects _systemObjects;
		

		public DebuggerThread(IDebugClient client, Symbols symbols)
		{
			_symbols = symbols;
			_control5 = (IDebugControl5) client;
			_advanced2 = (IDebugAdvanced2)client;
			_systemObjects = (IDebugSystemObjects)client;		
		}

		public unsafe ThreadContext GetThreadContext(ThreadInfo threadInfo)
		{
			
			var res = _systemObjects.SetCurrentThreadId(threadInfo.ThreadId);
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugSystemObjects.SetCurrentThreadId");
			}

			uint size = ContextSizeX64;
			byte[] context;
			do
			{
				context = new byte[size];
				fixed (byte* buffer = context)
				{
					res = _advanced2.GetThreadContext(new IntPtr(buffer), size);
				}
				if (res == InvalidParameter)
				{
					size *= 2;
				}
			} while (res == InvalidParameter);
			if(res != 0) {
				ThrowDebuggerException(res, "IDebugAdvanced2.GetThreadContext");
			}
			return new ThreadContext(context);
		}

		private unsafe  List<FrameInfo> GetCallstackFrames(ThreadContext context)
		{
			const uint maxFrames = 1024;
			var frames = new DEBUG_STACK_FRAME_EX[maxFrames];			
			byte[] framesBuf = new byte[maxFrames * ContextSizeX64];
			int res = -1;
			uint filled = 0;
			var contextBuffer = context.Context;
			
			fixed (byte* buf = contextBuffer)
			fixed (byte* framesContext = framesBuf)
			{												
				res = _control5.GetContextStackTraceEx(new IntPtr(buf), (uint) contextBuffer.Length, frames, frames.Length,
					(IntPtr)framesContext, (uint)framesBuf.Length, ContextSizeX64, out filled);													
			}
			
			if (res != 0)
			{
				ThrowDebuggerException(res, "IDebugControl5.GetContextStackTraceEx");
			}
			var retVal = new List<FrameInfo>((int) filled);
			for (int i = 0; i < filled; ++i)
			{
				var frameContext = new ArraySegment<byte>(framesBuf, i * ContextSizeX64, ContextSizeX64);
				retVal.Add(new FrameInfo(frames[i], frameContext));
			}
			return retVal;
		}

		public DebugThread GetCallstack(ThreadInfo threadInfo)
		{
			var context = GetThreadContext(threadInfo);
			var frames = GetCallstackFrames(context);
			var f = new List<StackFrame>(frames.Count);
			var retVal = new DebugThread(f, threadInfo);
			f.AddRange(frames.Select(frame => ToStackFrame(frame, retVal)));
			return retVal;
		}

		private StackFrame ToStackFrame(FrameInfo frameInfo, DebugThread thread)
		{
			ulong displacement;
			var builder = new StringBuilder(256);
			var frame = frameInfo.StackFrame;
			_symbols.GetSymbolNameByOffset(frame.InstructionOffset, ref builder, out displacement);
			var name = builder.ToString();

			return new StackFrame(frame.ReturnOffset, frame.InstructionOffset, frame.FrameOffset, (ushort)frame.FrameNumber, string.Intern(name), displacement, thread);
		}
	}

	internal struct ThreadInfo
	{
		public uint ThreadId;
		public uint SystemThreadId;

		public ThreadInfo(uint threadId, uint sysId)
		{
			ThreadId = threadId;
			SystemThreadId = sysId;
		}
	}

	internal struct FrameInfo
	{
		public DEBUG_STACK_FRAME_EX StackFrame { get; }
		public ArraySegment<byte> FrameContext { get; }

		public FrameInfo(DEBUG_STACK_FRAME_EX stackFrame, ArraySegment<byte> frameContext)
		{
			StackFrame = stackFrame;
			FrameContext = frameContext;
		}
	}

	class Scope
	{
		//GetScopeSymbolGroup
	}

	internal class SystemObjects
	{
		private readonly IDebugSystemObjects2 _systemObjects2;

		public SystemObjects(IDebugSystemObjects2 systemObjects2)
		{
			_systemObjects2 = systemObjects2;
		}

		public uint ThreadCount
		{
			get
			{
				uint retVal;
				var res = _systemObjects2.GetNumberThreads(out retVal);
				if (res != 0)
				{
					ThrowDebuggerException(res, "IDebugSystemObjects2.GetNumberThreads");
				}
				return retVal;
			}
		}

		public ThreadInfo[] ThreadInfos
		{
			get
			{
				var threadCount = ThreadCount;
				var debuggerThreadId = new uint[threadCount];
				var threadSysIds = new uint[threadCount];
				_systemObjects2.GetThreadIdsByIndex(0, threadCount, debuggerThreadId, threadSysIds);
				var retVal = new ThreadInfo[threadCount];
				for (uint i = 0; i < threadCount; ++i)
				{
					retVal[i] = new ThreadInfo(debuggerThreadId[i], threadSysIds[i]);
				}
				return retVal;
			}
		}

		public ThreadInfo CurrentThreadInfo
		{
			get
			{				
				uint threadId;
				_systemObjects2.GetCurrentThreadId(out threadId);
				uint sysId;
				_systemObjects2.GetCurrentThreadSystemId(out sysId);

				return new ThreadInfo(threadId, sysId);
			}
		}
	}
}