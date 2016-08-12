using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.Diagnostics.Runtime.Interop;
using PSExt.Commands;
using Xunit;

namespace PSExtTests
{
	public class DebuggerTests
	{
		
		private readonly IDebugClient6 _client6;
		public DebuggerTests()
		{
			DbgEngine.AddDllDirectory(@"C:\Program Files (x86)\Windows Kits\10\Debuggers\x64\");
			IDebugClient client;
			var iidIDebugClient = new Guid("27fe5639-8407-4f47-8364-ee118fb08ac8");
			int res = DbgEngine.DebugCreate(iidIDebugClient, out client);
			if (res != 0)
			{
				throw new Exception("Unable to create debugger");
			}
			_client6 = (IDebugClient6) client;
			var sym = (IDebugSymbols3) client;
			//res = _client6.OpenDumpFileWide(@"C:\Users\sgustafsson\Source\Repos\PSExt\dmp\Sample.dmp", 0);
			res = _client6.OpenDumpFileWide(@"C:\temp\CrashDump\crash.dmp", 0);
			if (res != 0)
			{
				throw new Exception("Unable to open dump");
			}
			var ctrl = (IDebugControl) _client6;
			res = ctrl.WaitForEvent(DEBUG_WAIT.DEFAULT, 5000);
			sym.SetSymbolPathWide(@"C:\Users\sgustafsson\Source\Repos\PSExt\dmp;d:\symbols");
			res = sym.Reload("/f");
			if (res != 0)
			{
				throw new Exception("loading symbols fails");
			}			
		}	

		[Fact]
		public void GetModules()
		{			
			var d = new PSExt.Debugger(_client6);
			var m = d.GetModules();
			Assert.Equal(13, m.Count);
		}

		[Fact]
		public void GetCallstack()
		{			
			var d = new PSExt.Debugger(_client6);
			var m = d.GetCallstack(true);
			Assert.Equal(4, m.Count);
		}

		[Fact]
		public void GetVariablesOneLevel()
		{
			var d = new PSExt.Debugger(_client6);
			var m = d.GetCallstack(true);
			var frame = m[0].Frames[0];			
			var vars = d.GetVariables(frame, 1);
			Assert.Equal(9, vars.Count);

		}

		[Fact]
		public void EnterDebuggerContextOnDump()
		{
			var iss = InitialSessionState.CreateDefault2();
			iss.ImportPSModule(new[] { typeof(DbgBaseCmdlet).Assembly.Location });
			
			using (var ps = PowerShell.Create(iss))
			{
				var script = @"
Enter-DebugContext -DumpPath D:\temp\CrashDump\239121\crash.dmp -SymbolPath 'srv*d:\symbols*http://dice-symproxy/TUNSymbol'
k -all
Exit-DebugContext
";
				ps.Commands.AddScript(script, true);
				var res = ps.Invoke();
				if (ps.Streams.Error.Count > 0)
				{
					Assert.True(false, ps.Streams.Error[0].ToString());
				}
			}

		}

	}
}