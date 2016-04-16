using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Interop;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace PSExtTests
{
	public class DbgEngine
	{		
		[DllImport("Kernel32.dll")]
		public static extern IntPtr AddDllDirectory(string newDirectory);

		[DllImport("dbgeng.dll")]
		public static extern int DebugCreate(Guid refId, out IDebugClient ptr);
	}
}
