using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Diagnostics.Runtime.Interop;

namespace PSExt
{
	[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
	internal class BreakpointManager
	{
		private readonly IDebugControl5 _control5;
		private readonly StringBuilder _builder = new StringBuilder(256);
		private readonly Symbols _symbols;

		public BreakpointManager(IDebugClient6 client)
		{
			_control5 = (IDebugControl5) client;
			_symbols = new Symbols((IDebugSymbols3) client);
		}

		private uint GetBreakpointCount()
		{
			uint number;
			var res = _control5.GetNumberBreakpoints(out number);
			if (res != 0)
			{
				ExceptionHelper.ThrowDebuggerException(res, "IDebugControl5::GetNumberBreakpoints");
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
				ExceptionHelper.ThrowDebuggerException(res, "IDebugControl5.AddBreakpoint2");
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
				ExceptionHelper.ThrowDebuggerException(res, "IDebugBreakpoint2.GetId");
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
					ExceptionHelper.ThrowDebuggerException(res, "_control5.GetBreakpointParameters");
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
					ExceptionHelper.ThrowDebuggerException(res, "IDebugBreakpoint2.GetCommandWide");
				}
			}
			return _builder.ToString();
		}


		private string GetOffsetExpression(IDebugBreakpoint2 bp, uint expressionSize)
		{			
			if (expressionSize != 0)
			{
				_builder.EnsureCapacity((int) expressionSize);
				var res = bp.GetOffsetExpressionWide(_builder,_builder.Capacity, out expressionSize);
				if (res != 0)
				{
					ExceptionHelper.ThrowDebuggerException(res, "IDebugBreakpoint2.GetOffsetExpressionWide");
				}
			}
			return _builder.ToString();
		}
		
		public IList<BreakpointData> AddBreakpoints(BreakpointData data)
		{
			var ids = new List<uint>();
			uint bpid;
			if (data.Expression != null)
			{

				var pattern = data.Expression;
				IList<SymbolSearchResult> matches = _symbols.GetMatchingSymbols(pattern);
				if (matches.Count == 0)
				{
					// do nothing
				}
				else {
					foreach (var match in matches)
					{
						bpid = AddBreakpointOffset(match.Offset, data);
						ids.Add(bpid);
					}
				}
			}
			else {
				bpid = AddBreakpointOffset(data.Offset, data);
				ids.Add(bpid);

			}
			var bps = GetBreakpoints();

			return bps.Where(bp => ids.Contains(bp.Id)).ToList();
		}
	}
}