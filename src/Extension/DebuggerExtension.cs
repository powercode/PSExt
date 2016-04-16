using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Microsoft.Diagnostics.Runtime.Interop;
using PSExt.Host;
using RGiesecke.DllExport;

namespace PSExt.Extension
{
	public class DebuggerExtension
	{		
		public static IDebugClient DebugClient { get; private set; }

		private static IDebugger _debugger;
		private static IDebugger Debugger => _debugger ?? (_debugger = new DebuggerProxy(new Debugger(DebugClient), Dispatcher));

		private static DebuggerDispatcher _dispatcher;
		private static DebuggerDispatcher Dispatcher => _dispatcher ?? (_dispatcher = new DebuggerDispatcher());

		private static ExitManager _exitManager;
		private static ExitManager ExitManager => _exitManager ?? (_exitManager = new ExitManager());

		private static PSSession _powerShellSession;
		private static PSSession PowerShellSession => _powerShellSession ??
													 (_powerShellSession = new PSSession(Debugger, new DbgPsHost(Debugger, ExitManager), Dispatcher));

		private static bool InitApi(IntPtr ptrClient)
		{
			// On our first call to the API:
			//   1. Store a copy of IDebugClient in DebugClient.
			//   2. Replace Console's output stream to be the debugger window.
			//   3. Create an instance of DataTarget using the IDebugClient.
			if (DebugClient == null)
			{
				var client = Marshal.GetUniqueObjectForIUnknown(ptrClient);
				DebugClient = (IDebugClient) client;

				var stream = new StreamWriter(new DbgEngStream(DebugClient)) {AutoFlush = true};
				Console.SetOut(stream);								
			}

			return true;
		}

		[DllExport("DebugExtensionInitialize")]
		public static int DebugExtensionInitialize(out uint version, out uint flags)
		{
			// Set the extension version to 1, which expects exports with this signature:
			//      void _stdcall function(IDebugClient *client, const char *args)
			version = DEBUG_EXTENSION_VERSION(1, 0);
			flags = 0;
			return 0;
		}

		[DllExport("DebugExtensionUninitialize")]		
		public static int DebugExtensionUninitialize()
		{
			PowerShellSession.Dispose();
			Marshal.ReleaseComObject(DebugClient);
			DebugClient = null;			
			return 0;
		}


		[DllExport("ps")]
		public static void PS(IntPtr client, [MarshalAs(UnmanagedType.LPStr)] string args)
		{
			// Must be the first thing in our extension.
			if (!InitApi(client))
				return;

			PowerShellSession.Invoke(args);
		}


		private static uint DEBUG_EXTENSION_VERSION(uint major, uint minor)
		{
			return ((major & 0xffff) << 16) | (minor & 0xffff);
		}

	}

	internal class DbgEngStream : Stream
	{
		private readonly IDebugClient _client;
		private readonly IDebugControl _control;

		public DbgEngStream(IDebugClient client)
		{
			_client = client;
			// ReSharper disable once SuspiciousTypeConversion.Global
			_control = (IDebugControl) client;
		}

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => -1;

		public override long Position
		{
			get { return 0; }
			set { }
		}

		public void Clear()
		{
			while (Marshal.ReleaseComObject(_client) > 0)
			{
			}
			while (Marshal.ReleaseComObject(_control) > 0)
			{
			}
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			var enc = new UTF8Encoding();
			var str = enc.GetString(buffer, offset, count);
			_control.ControlledOutput(DEBUG_OUTCTL.ALL_CLIENTS, DEBUG_OUTPUT.NORMAL, str);
		}
	}
}