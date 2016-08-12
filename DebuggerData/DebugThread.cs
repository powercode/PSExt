using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DebugData
{
	public class DebugThread : IComparable<DebugThread>, IComparable
	{
		public DebugThread(List<StackFrame> frames, uint systemThreadId, uint threadId)
		{
			Frames = frames;
			ThreadId = systemThreadId;
			ThreadNumber = threadId;
		}

		public uint ThreadId { get; }
		public uint ThreadNumber { get; }

		
		public override string ToString()
		{
			return $"#{ThreadNumber} ThreadId:{ThreadId}";
		}

		public List<StackFrame> Frames { get; }

		public bool Matches(string pattern)
		{
			return Frames.Any(f => f.Matches(pattern));
		}

		public bool MatchesAll(string[] patterns)
		{
			return patterns.All(Matches);
		}

		public bool MatchesAny(string[] patterns)
		{
			return patterns.Any(Matches);
		}

		public bool Contains(string pattern)
		{
			return Frames.Any(f => f.Contains(pattern));
		}

		public bool ContainsAll(string[] patterns)
		{
			return patterns.All(Contains);
		}

		public bool ContainsAny(string[] patterns)
		{
			return patterns.Any(Contains);
		}

		int IComparable<DebugThread>.CompareTo(DebugThread other)
		{
			return ThreadId.CompareTo(other.ThreadId);
		}


		int IComparable.CompareTo(object obj)
		{
			return ((IComparable<DebugThread>)this).CompareTo((DebugThread)obj);
		}


	}
}