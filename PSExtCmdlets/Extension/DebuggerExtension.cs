using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Diagnostics.Runtime.Interop;
using RGiesecke.DllExport;

namespace PSExt.Extension
{
	public partial class DebuggerExtension
	{
		private static readonly string PSExtDll = "PSExtCmdlets";

		static DebuggerExtension()
		{
			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
		}

		public static IDebugClient DebugClient { get; private set; }

		private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{
			if (!args.Name.Contains(PSExtDll)) return null;

			var codebase = Assembly.GetExecutingAssembly().CodeBase;

			if (codebase.StartsWith("file://"))
				codebase = codebase.Substring(8).Replace('/', '\\');

			var directory = Path.GetDirectoryName(codebase);
			var path = Path.Combine(directory, PSExtDll) + ".dll";
			return Assembly.LoadFile(path);
		}

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
		public static int DebugExtensionInitialize(ref uint version, ref uint flags)
		{
			// Set the extension version to 1, which expects exports with this signature:
			//      void _stdcall function(IDebugClient *client, const char *args)
			version = DEBUG_EXTENSION_VERSION(1, 0);
			flags = 0;
			return 0;
		}

		private static uint DEBUG_EXTENSION_VERSION(uint Major, uint Minor)
		{
			return ((Major & 0xffff) << 16) | (Minor & 0xffff);
		}
	}

	internal class DbgEngStream : Stream
	{
		private readonly IDebugClient _client;
		private readonly IDebugControl _control;

		public DbgEngStream(IDebugClient client)
		{
			_client = client;
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