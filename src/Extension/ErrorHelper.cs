namespace PSExt
{
	internal class ErrorHelper
	{
		public const int InvalidParameter = unchecked((int)0x80070057);
		public const int NoInterface = unchecked((int)0x80004002);
		public static void ThrowDebuggerException(int statusCode, string failingMethod)
		{
			throw new DebuggerException(statusCode, failingMethod);
		}
	}
}