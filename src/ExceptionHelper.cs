namespace PSExt
{
	internal class ExceptionHelper
	{
		public static void ThrowDebuggerException(int statusCode, string failingMethod)
		{
			throw new DebuggerException(statusCode, failingMethod);
		}
	}
}