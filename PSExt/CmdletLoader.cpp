#include "CmdletLoader.h"
using namespace System;
using namespace System::Reflection;
using namespace System::IO;
ref class CmdletLoader
{
public:
	 static void AddAssemblyLoadResolver(){
		 auto currentDomain = System::AppDomain::CurrentDomain;
		 currentDomain->AssemblyResolve += gcnew ResolveEventHandler(&CmdletLoader::OnAssemblyResolve);
	}	
	 static Assembly ^ CmdletLoader::OnAssemblyResolve(Object ^, ResolveEventArgs ^args)
	 {
		 if (!args->Name->StartsWith("PSExtCmdlets")){
			 return nullptr;
		 }
		 String^ path = Path::GetDirectoryName(Assembly::GetCallingAssembly()->Location);		 
		 auto resolveName = gcnew AssemblyName(args->Name);
		 String^ assemblyName = resolveName->Name + ".dll";
		 String^  assemblyPath = Path::Combine(path, assemblyName);
		 if (!File::Exists(assemblyPath)) return nullptr;
		 auto assembly = Assembly::LoadFrom(assemblyPath);
		 return assembly;
	 }

};


void LoadCmdletAssembly(){
	CmdletLoader::AddAssemblyLoadResolver();
}

