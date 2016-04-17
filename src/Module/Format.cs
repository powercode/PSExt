using System.Management.Automation;

namespace PSExt
{
	public static class Format
	{
		public static string ToDisplayAddress(PSObject u)
		{
			var ul = (ulong) u.BaseObject;
			var s = ul.ToString("x16");
			return $"{s.Substring(0, 8)}`{s.Substring(8)}";
		}
	}
}