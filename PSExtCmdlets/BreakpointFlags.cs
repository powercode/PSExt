using System;

namespace PSExt
{
	[Flags]
	public enum BreakpointFlags
	{
		// Go-only breakpoints are only active when
		// the engine is in unrestricted execution
		// mode.  They do not fire when the engine
		// is stepping.
		GoOnly = 1 << 0,
		// A breakpoint is flagged as deferred as long as
		// its offset expression cannot be evaluated.
		// A deferred breakpoint is not active.
		Deferred = 1 << 1,
		Enabled = 1 << 2,
		// The adder-only flag does not affect breakpoint
		// operation.  It is just a marker to restrict
		// output and notifications for the breakpoint to
		// the client that added the breakpoint.  Breakpoint
		// callbacks for adder-only breaks will only be delivered
		// to the adding client.  The breakpoint can not
		// be enumerated and accessed by other clients.
		AdderOnly = 1 << 3,
		// One-shot breakpoints varmatically clear themselves
		// the first time they are hit.
		OneShot = 1 << 4,
	};
}