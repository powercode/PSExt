using System;

namespace PSExt
{
	public class SimpleDbgModule{

		public UInt64 Start{ get; private set; }
		public UInt64 End{ get; private set; }
		public String ModuleName { get; private set; }
		public SimpleDbgModule(UInt64 start, UInt64 end, String moduleName)		
		{		
			Start = start;
			End = end;
			ModuleName = moduleName;
		}

		public override String ToString(){
			return ModuleName;
		}
	};
}