#pragma once
#using <PSExtCmdlets.dll>

ref class Modules {	
public:
	static System::Collections::Generic::IList<PSExt::ModuleData^>^ GetModules();	

};
