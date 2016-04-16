using System;
using System.Diagnostics;
using System.IO;

namespace PSExt
{
	[DebuggerDisplay("{ModuleName}")]
	public class ModuleData
	{
		public ModuleData(string moduleName, string imageName, string loadedImageName, string loadedPdbName, ulong baseOfImage,
			uint imageSize, uint timeDateStamp, uint checkSum, uint numSyms, uint symType,
			Guid pdbSig70, uint pdbAge, bool pdbUnmatched, bool lineNumbers, bool globalSymbols, bool typeInfo,
			bool sourceIndexed, bool publics, uint machineType)
		{
			ModuleName = moduleName;
			ImageName = imageName;
			LoadedImageName = loadedImageName;
			LoadedPdbName = loadedPdbName;
			BaseOfImage = baseOfImage;
			ImageSize = imageSize;
			TimeDateStamp = timeDateStamp;
			CheckSum = checkSum;
			NumSyms = numSyms;
			SymType = (SymType) symType;
			PdbSig70 = pdbSig70;
			PdbAge = pdbAge;
			PdbUnmatched = pdbUnmatched;
			LineNumbers = lineNumbers;
			GlobalSymbols = globalSymbols;
			TypeInfo = typeInfo;
			SourceIndexed = sourceIndexed;
			Publics = publics;
			MachineType = (ImageFileMachineType) machineType;
		}

		public string ModuleName { get; } // module name
		public string ImageName { get; } // image name									  
		public string LoadedImageName { get; } // symbol file name
		public string LoadedPdbName { get; } // pdb file name
		public ulong BaseOfImage { get; } // base load address of module
		public uint ImageSize { get; } // virtual size of the loaded module
		public uint TimeDateStamp { get; } // date/time stamp from pe header		
		public uint CheckSum { get; } // checksum from the pe header
		public uint NumSyms { get; } // number of symbols in the symbol table
		public SymType SymType { get; } // type of symbols loaded		
		public Guid PdbSig70 { get; } // Signature of PDB (VC 7 and up)
		public uint PdbAge { get; } // DBI age of pdb
		public bool PdbUnmatched { get; } // loaded an unmatched pdb		
		public bool LineNumbers { get; } // we have line number information
		public bool GlobalSymbols { get; } // we have internal symbol information
		public bool TypeInfo { get; } // we have type information									 
		public bool SourceIndexed { get; } // pdb supports source server
		public bool Publics { get; } // contains public symbols									 
		public ImageFileMachineType MachineType { get; } // IMAGE_FILE_MACHINE_XXX from ntimage.h and winnt.h			

		public DateTime Built => new DateTime((62135600400 + TimeDateStamp)*10000000, DateTimeKind.Utc);

		public ulong EndOfImage => BaseOfImage + ImageSize;

		public FileVersionInfo VersionInfo
			=> File.Exists(LoadedImageName) ? FileVersionInfo.GetVersionInfo(LoadedImageName) : null;

		public override string ToString() => ModuleName;
	}
}