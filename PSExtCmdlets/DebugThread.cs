using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PSExt
{
	public class DebugThread
	{
		public DebugThread(List<StackFrame> frames)
		{
			Frames = frames;
			ThreadId = 4711;
			ThreadNumber = 1;
		}

		public int ThreadId { get; }
		public int ThreadNumber { get; }

		public override string ToString()
		{
			return $"#{ThreadNumber} ThreadId:{ThreadId}";
		}

		public List<StackFrame> Frames { get; }

		public bool Matches(string pattern)
		{
			return Frames.Any(f => Regex.IsMatch(f.Name, pattern));
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
			return Frames.Any(f => f.Name.IndexOf(pattern, StringComparison.Ordinal) != -1);
		}

		public bool ContainsAll(string[] patterns)
		{
			return patterns.All(Contains);
		}

		public bool ContainsAny(string[] patterns)
		{
			return patterns.Any(Contains);
		}
	}
}