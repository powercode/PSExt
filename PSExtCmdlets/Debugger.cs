using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Diagnostics.Runtime.Interop;

namespace PSExt
{
	internal class Debugger : IDebugger
	{
		
		private readonly IDebugClient6 _client;
		private readonly IDebugControl5 _control5;
		private readonly IDebugAdvanced2 _advanced2;

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
			throw new System.NotImplementedException();
		}

		public IList<BreakpointData> AddBreakpoints(BreakpointData data)
		{
			throw new System.NotImplementedException();
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

		public DebugThread GetCallstack()
		{
			throw new System.NotImplementedException();
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
}