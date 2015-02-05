# PSExt
Windows Debuggers extension for PowerShell

PSExt uses engextcpp, a helper library to write debugger extensions, and a PowerShell host to enable running powershell commands
at the debugger prompt.

The goal is to make searching memory, symbols, stacks and registers much easier to automate.

Examples of usage in cdb (or windbg):

<pre>
<code>
.load <path to>\psext.dll
!ps Invoke-DbgCommand -Command k | Select-String MyFunction -ov func
!ps $func | Format-List
</code>
</pre>

