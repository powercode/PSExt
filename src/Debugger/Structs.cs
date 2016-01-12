// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace Microsoft.Diagnostics.Runtime.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct IMAGEHLP_MODULEW64
	{
		private const int MAX_PATH = 260;

		public uint SizeOfStruct;
		public ulong BaseOfImage;
		public uint ImageSize;
		public uint TimeDateStamp;
		public uint CheckSum;
		public uint NumSyms;
		public DEBUG_SYMTYPE SymType;
		private fixed short _ModuleName [32];
		private fixed short _ImageName [256];
		private fixed short _LoadedImageName [256];
		private fixed short _LoadedPdbName [256];
		public uint CVSig;
		public fixed short CVData [MAX_PATH*3];
		public uint PdbSig;
		public Guid PdbSig70;
		public uint PdbAge;
		private uint _bPdbUnmatched; /* BOOL */
		private uint _bDbgUnmatched; /* BOOL */
		private uint _bLineNumbers; /* BOOL */
		private uint _bGlobalSymbols; /* BOOL */
		private uint _bTypeInfo; /* BOOL */
		private uint _bSourceIndexed; /* BOOL */
		private uint _bPublics; /* BOOL */
		public uint MachineType; // IMAGE_FILE_MACHINE_XXX from ntimage.h and winnt.h
		private readonly uint _Reserved; // Padding - don't remove.

		public bool PdbUnmatched
		{
			get { return _bPdbUnmatched != 0; }
			set { _bPdbUnmatched = value ? 1U : 0U; }
		}

		public bool DbgUnmatched
		{
			get { return _bDbgUnmatched != 0; }
			set { _bDbgUnmatched = value ? 1U : 0U; }
		}

		public bool LineNumbers
		{
			get { return _bLineNumbers != 0; }
			set { _bLineNumbers = value ? 1U : 0U; }
		}

		public bool GlobalSymbols
		{
			get { return _bGlobalSymbols != 0; }
			set { _bGlobalSymbols = value ? 1U : 0U; }
		}

		public bool TypeInfo
		{
			get { return _bTypeInfo != 0; }
			set { _bTypeInfo = value ? 1U : 0U; }
		}

		public bool SourceIndexed
		{
			get { return _bSourceIndexed != 0; }
			set { _bSourceIndexed = value ? 1U : 0U; }
		}

		public bool Publics
		{
			get { return _bPublics != 0; }
			set { _bPublics = value ? 1U : 0U; }
		}

		public string ModuleName
		{
			get
			{
				fixed (short* moduleNamePtr = _ModuleName)
				{
					return Marshal.PtrToStringUni((IntPtr) moduleNamePtr);
				}
			}
		}

		public string ImageName
		{
			get
			{
				fixed (short* imageNamePtr = _ImageName)
				{
					return Marshal.PtrToStringUni((IntPtr) imageNamePtr);
				}
			}
		}

		public string LoadedImageName
		{
			get
			{
				fixed (short* loadedImageNamePtr = _LoadedImageName)
				{
					return Marshal.PtrToStringUni((IntPtr) loadedImageNamePtr);
				}
			}
		}

		public string LoadedPdbName
		{
			get
			{
				fixed (short* loadedPdbNamePtr = _LoadedPdbName)
				{
					return Marshal.PtrToStringUni((IntPtr) loadedPdbNamePtr);
				}
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_THREAD_BASIC_INFORMATION
	{
		public DEBUG_TBINFO Valid;
		public uint ExitStatus;
		public uint PriorityClass;
		public uint Priority;
		public ulong CreateTime;
		public ulong ExitTime;
		public ulong KernelTime;
		public ulong UserTime;
		public ulong StartOffset;
		public ulong Affinity;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_READ_USER_MINIDUMP_STREAM
	{
		public uint StreamType;
		public uint Flags;
		public ulong Offset;
		public IntPtr Buffer;
		public uint BufferSize;
		public uint BufferUsed;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_GET_TEXT_COMPLETIONS_IN
	{
		public DEBUG_GET_TEXT_COMPLETIONS Flags;
		public uint MatchCountLimit;
		public ulong Reserved0;
		public ulong Reserved1;
		public ulong Reserved2;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_GET_TEXT_COMPLETIONS_OUT
	{
		public DEBUG_GET_TEXT_COMPLETIONS Flags;
		public uint ReplaceIndex;
		public uint MatchCount;
		public uint Reserved1;
		public ulong Reserved2;
		public ulong Reserved3;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_CACHED_SYMBOL_INFO
	{
		public ulong ModBase;
		public ulong Arg1;
		public ulong Arg2;
		public uint Id;
		public uint Arg3;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_CREATE_PROCESS_OPTIONS
	{
		public DEBUG_CREATE_PROCESS CreateFlags;
		public DEBUG_ECREATE_PROCESS EngCreateFlags;
		public uint VerifierFlags;
		public uint Reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct EXCEPTION_RECORD64
	{
		public uint ExceptionCode;
		public uint ExceptionFlags;
		public ulong ExceptionRecord;
		public ulong ExceptionAddress;
		public uint NumberParameters;
		public uint __unusedAlignment;
		public fixed ulong ExceptionInformation [15]; //EXCEPTION_MAXIMUM_PARAMETERS
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_BREAKPOINT_PARAMETERS
	{
		public ulong Offset;
		public uint Id;
		public DEBUG_BREAKPOINT_TYPE BreakType;
		public uint ProcType;
		public DEBUG_BREAKPOINT_FLAG Flags;
		public uint DataSize;
		public DEBUG_BREAKPOINT_ACCESS_TYPE DataAccessType;
		public uint PassCount;
		public uint CurrentPassCount;
		public uint MatchThread;
		public uint CommandSize;
		public uint OffsetExpressionSize;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_REGISTER_DESCRIPTION
	{
		public DEBUG_VALUE_TYPE Type;
		public DEBUG_REGISTER Flags;
		public ulong SubregMaster;
		public ulong SubregLength;
		public ulong SubregMask;
		public ulong SubregShift;
		public ulong Reserved0;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct I64PARTS32
	{
		[FieldOffset(0)] public uint LowPart;
		[FieldOffset(4)] public uint HighPart;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct F128PARTS64
	{
		[FieldOffset(0)] public ulong LowPart;
		[FieldOffset(8)] public ulong HighPart;
	}

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct DEBUG_VALUE
	{
		[FieldOffset(0)] public byte I8;
		[FieldOffset(0)] public ushort I16;
		[FieldOffset(0)] public uint I32;
		[FieldOffset(0)] public ulong I64;
		[FieldOffset(8)] public uint Nat;
		[FieldOffset(0)] public float F32;
		[FieldOffset(0)] public double F64;
		[FieldOffset(0)] public fixed byte F80Bytes [10];
		[FieldOffset(0)] public fixed byte F82Bytes [11];
		[FieldOffset(0)] public fixed byte F128Bytes [16];
		[FieldOffset(0)] public fixed byte VI8 [16];
		[FieldOffset(0)] public fixed ushort VI16 [8];
		[FieldOffset(0)] public fixed uint VI32 [4];
		[FieldOffset(0)] public fixed ulong VI64 [2];
		[FieldOffset(0)] public fixed float VF32 [4];
		[FieldOffset(0)] public fixed double VF64 [2];
		[FieldOffset(0)] public I64PARTS32 I64Parts32;
		[FieldOffset(0)] public F128PARTS64 F128Parts64;
		[FieldOffset(0)] public fixed byte RawBytes [24];
		[FieldOffset(24)] public uint TailOfRawBytes;
		[FieldOffset(28)] public DEBUG_VALUE_TYPE Type;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct DEBUG_MODULE_PARAMETERS
	{
		public ulong Base;
		public uint Size;
		public uint TimeDateStamp;
		public uint Checksum;
		public DEBUG_MODULE Flags;
		public DEBUG_SYMTYPE SymbolType;
		public uint ImageNameSize;
		public uint ModuleNameSize;
		public uint LoadedImageNameSize;
		public uint SymbolFileNameSize;
		public uint MappedImageNameSize;
		public fixed ulong Reserved [2];
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct DEBUG_STACK_FRAME
	{
		public ulong InstructionOffset;
		public ulong ReturnOffset;
		public ulong FrameOffset;
		public ulong StackOffset;
		public ulong FuncTableEntry;
		public fixed ulong Params [4];
		public fixed ulong Reserved [6];
		public uint Virtual;
		public uint FrameNumber;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct DEBUG_STACK_FRAME_EX
	{
		/* DEBUG_STACK_FRAME */
		public ulong InstructionOffset;
		public ulong ReturnOffset;
		public ulong FrameOffset;
		public ulong StackOffset;
		public ulong FuncTableEntry;
		public fixed ulong Params [4];
		public fixed ulong Reserved [6];
		public uint Virtual;
		public uint FrameNumber;

		/* DEBUG_STACK_FRAME_EX */
		public uint InlineFrameContext;
		public uint Reserved1;

		public DEBUG_STACK_FRAME_EX(DEBUG_STACK_FRAME dsf)
		{
			InstructionOffset = dsf.InstructionOffset;
			ReturnOffset = dsf.ReturnOffset;
			FrameOffset = dsf.FrameOffset;
			StackOffset = dsf.StackOffset;
			FuncTableEntry = dsf.FuncTableEntry;
			fixed (ulong* pParams = Params)
			{
				for (var i = 0; i < 4; ++i)
					pParams[i] = dsf.Params[i];
			}
			fixed (ulong* pReserved = Reserved)
			{
				for (var i = 0; i < 6; ++i)
					pReserved[i] = dsf.Reserved[i];
			}
			Virtual = dsf.Virtual;
			FrameNumber = dsf.FrameNumber;
			InlineFrameContext = 0xFFFFFFFF;
			Reserved1 = 0;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_SYMBOL_PARAMETERS
	{
		public ulong Module;
		public uint TypeId;
		public uint ParentSymbol;
		public uint SubElements;
		public DEBUG_SYMBOL Flags;
		public ulong Reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WINDBG_EXTENSION_APIS /*32 or 64; both are defined the same in managed code*/
	{
		public uint nSize;
		public IntPtr lpOutputRoutine;
		public IntPtr lpGetExpressionRoutine;
		public IntPtr lpGetSymbolRoutine;
		public IntPtr lpDisasmRoutine;
		public IntPtr lpCheckControlCRoutine;
		public IntPtr lpReadProcessMemoryRoutine;
		public IntPtr lpWriteProcessMemoryRoutine;
		public IntPtr lpGetThreadContextRoutine;
		public IntPtr lpSetThreadContextRoutine;
		public IntPtr lpIoctlRoutine;
		public IntPtr lpStackTraceRoutine;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_SPECIFIC_FILTER_PARAMETERS
	{
		public DEBUG_FILTER_EXEC_OPTION ExecutionOption;
		public DEBUG_FILTER_CONTINUE_OPTION ContinueOption;
		public uint TextSize;
		public uint CommandSize;
		public uint ArgumentSize;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_EXCEPTION_FILTER_PARAMETERS
	{
		public DEBUG_FILTER_EXEC_OPTION ExecutionOption;
		public DEBUG_FILTER_CONTINUE_OPTION ContinueOption;
		public uint TextSize;
		public uint CommandSize;
		public uint SecondCommandSize;
		public uint ExceptionCode;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_HANDLE_DATA_BASIC
	{
		public uint TypeNameSize;
		public uint ObjectNameSize;
		public uint Attributes;
		public uint GrantedAccess;
		public uint HandleCount;
		public uint PointerCount;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct MEMORY_BASIC_INFORMATION64
	{
		public ulong BaseAddress;
		public ulong AllocationBase;
		public PAGE AllocationProtect;
		public uint __alignment1;
		public ulong RegionSize;
		public MEM State;
		public PAGE Protect;
		public MEM Type;
		public uint __alignment2;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_NT_HEADERS32
	{
		[FieldOffset(0)] public uint Signature;
		[FieldOffset(4)] public IMAGE_FILE_HEADER FileHeader;
		[FieldOffset(24)] public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_NT_HEADERS64
	{
		[FieldOffset(0)] public uint Signature;
		[FieldOffset(4)] public IMAGE_FILE_HEADER FileHeader;
		[FieldOffset(24)] public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_FILE_HEADER
	{
		[FieldOffset(0)] public ushort Machine;
		[FieldOffset(2)] public ushort NumberOfSections;
		[FieldOffset(4)] public uint TimeDateStamp;
		[FieldOffset(8)] public uint PointerToSymbolTable;
		[FieldOffset(12)] public uint NumberOfSymbols;
		[FieldOffset(16)] public ushort SizeOfOptionalHeader;
		[FieldOffset(18)] public ushort Characteristics;
	}

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct IMAGE_DOS_HEADER
	{
		[FieldOffset(0)] public ushort e_magic; // Magic number
		[FieldOffset(2)] public ushort e_cblp; // Bytes on last page of file
		[FieldOffset(4)] public ushort e_cp; // Pages in file
		[FieldOffset(6)] public ushort e_crlc; // Relocations
		[FieldOffset(8)] public ushort e_cparhdr; // Size of header in paragraphs
		[FieldOffset(10)] public ushort e_minalloc; // Minimum extra paragraphs needed
		[FieldOffset(12)] public ushort e_maxalloc; // Maximum extra paragraphs needed
		[FieldOffset(14)] public ushort e_ss; // Initial (relative) SS value
		[FieldOffset(16)] public ushort e_sp; // Initial SP value
		[FieldOffset(18)] public ushort e_csum; // Checksum
		[FieldOffset(20)] public ushort e_ip; // Initial IP value
		[FieldOffset(22)] public ushort e_cs; // Initial (relative) CS value
		[FieldOffset(24)] public ushort e_lfarlc; // File address of relocation table
		[FieldOffset(26)] public ushort e_ovno; // Overlay number
		[FieldOffset(28)] public fixed ushort e_res [4]; // Reserved words
		[FieldOffset(36)] public ushort e_oemid; // OEM identifier (for e_oeminfo)
		[FieldOffset(38)] public ushort e_oeminfo; // OEM information; e_oemid specific
		[FieldOffset(40)] public fixed ushort e_res2 [10]; // Reserved words
		[FieldOffset(60)] public uint e_lfanew; // File address of new exe header
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_OPTIONAL_HEADER32
	{
		[FieldOffset(0)] public ushort Magic;
		[FieldOffset(2)] public byte MajorLinkerVersion;
		[FieldOffset(3)] public byte MinorLinkerVersion;
		[FieldOffset(4)] public uint SizeOfCode;
		[FieldOffset(8)] public uint SizeOfInitializedData;
		[FieldOffset(12)] public uint SizeOfUninitializedData;
		[FieldOffset(16)] public uint AddressOfEntryPoint;
		[FieldOffset(20)] public uint BaseOfCode;
		[FieldOffset(24)] public uint BaseOfData;
		[FieldOffset(28)] public uint ImageBase;
		[FieldOffset(32)] public uint SectionAlignment;
		[FieldOffset(36)] public uint FileAlignment;
		[FieldOffset(40)] public ushort MajorOperatingSystemVersion;
		[FieldOffset(42)] public ushort MinorOperatingSystemVersion;
		[FieldOffset(44)] public ushort MajorImageVersion;
		[FieldOffset(46)] public ushort MinorImageVersion;
		[FieldOffset(48)] public ushort MajorSubsystemVersion;
		[FieldOffset(50)] public ushort MinorSubsystemVersion;
		[FieldOffset(52)] public uint Win32VersionValue;
		[FieldOffset(56)] public uint SizeOfImage;
		[FieldOffset(60)] public uint SizeOfHeaders;
		[FieldOffset(64)] public uint CheckSum;
		[FieldOffset(68)] public ushort Subsystem;
		[FieldOffset(70)] public ushort DllCharacteristics;
		[FieldOffset(72)] public uint SizeOfStackReserve;
		[FieldOffset(76)] public uint SizeOfStackCommit;
		[FieldOffset(80)] public uint SizeOfHeapReserve;
		[FieldOffset(84)] public uint SizeOfHeapCommit;
		[FieldOffset(88)] public uint LoaderFlags;
		[FieldOffset(92)] public uint NumberOfRvaAndSizes;
		[FieldOffset(96)] public IMAGE_DATA_DIRECTORY DataDirectory0;
		[FieldOffset(104)] public IMAGE_DATA_DIRECTORY DataDirectory1;
		[FieldOffset(112)] public IMAGE_DATA_DIRECTORY DataDirectory2;
		[FieldOffset(120)] public IMAGE_DATA_DIRECTORY DataDirectory3;
		[FieldOffset(128)] public IMAGE_DATA_DIRECTORY DataDirectory4;
		[FieldOffset(136)] public IMAGE_DATA_DIRECTORY DataDirectory5;
		[FieldOffset(144)] public IMAGE_DATA_DIRECTORY DataDirectory6;
		[FieldOffset(152)] public IMAGE_DATA_DIRECTORY DataDirectory7;
		[FieldOffset(160)] public IMAGE_DATA_DIRECTORY DataDirectory8;
		[FieldOffset(168)] public IMAGE_DATA_DIRECTORY DataDirectory9;
		[FieldOffset(176)] public IMAGE_DATA_DIRECTORY DataDirectory10;
		[FieldOffset(284)] public IMAGE_DATA_DIRECTORY DataDirectory11;
		[FieldOffset(292)] public IMAGE_DATA_DIRECTORY DataDirectory12;
		[FieldOffset(300)] public IMAGE_DATA_DIRECTORY DataDirectory13;
		[FieldOffset(308)] public IMAGE_DATA_DIRECTORY DataDirectory14;
		[FieldOffset(316)] public IMAGE_DATA_DIRECTORY DataDirectory15;

		public static unsafe IMAGE_DATA_DIRECTORY* GetDataDirectory(IMAGE_OPTIONAL_HEADER32* header, int zeroBasedIndex)
		{
			return &header->DataDirectory0 + zeroBasedIndex;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_OPTIONAL_HEADER64
	{
		[FieldOffset(0)] public ushort Magic;
		[FieldOffset(2)] public byte MajorLinkerVersion;
		[FieldOffset(3)] public byte MinorLinkerVersion;
		[FieldOffset(4)] public uint SizeOfCode;
		[FieldOffset(8)] public uint SizeOfInitializedData;
		[FieldOffset(12)] public uint SizeOfUninitializedData;
		[FieldOffset(16)] public uint AddressOfEntryPoint;
		[FieldOffset(20)] public uint BaseOfCode;
		[FieldOffset(24)] public ulong ImageBase;
		[FieldOffset(32)] public uint SectionAlignment;
		[FieldOffset(36)] public uint FileAlignment;
		[FieldOffset(40)] public ushort MajorOperatingSystemVersion;
		[FieldOffset(42)] public ushort MinorOperatingSystemVersion;
		[FieldOffset(44)] public ushort MajorImageVersion;
		[FieldOffset(46)] public ushort MinorImageVersion;
		[FieldOffset(48)] public ushort MajorSubsystemVersion;
		[FieldOffset(50)] public ushort MinorSubsystemVersion;
		[FieldOffset(52)] public uint Win32VersionValue;
		[FieldOffset(56)] public uint SizeOfImage;
		[FieldOffset(60)] public uint SizeOfHeaders;
		[FieldOffset(64)] public uint CheckSum;
		[FieldOffset(68)] public ushort Subsystem;
		[FieldOffset(70)] public ushort DllCharacteristics;
		[FieldOffset(72)] public ulong SizeOfStackReserve;
		[FieldOffset(80)] public ulong SizeOfStackCommit;
		[FieldOffset(88)] public ulong SizeOfHeapReserve;
		[FieldOffset(96)] public ulong SizeOfHeapCommit;
		[FieldOffset(104)] public uint LoaderFlags;
		[FieldOffset(108)] public uint NumberOfRvaAndSizes;
		[FieldOffset(112)] public IMAGE_DATA_DIRECTORY DataDirectory0;
		[FieldOffset(120)] public IMAGE_DATA_DIRECTORY DataDirectory1;
		[FieldOffset(128)] public IMAGE_DATA_DIRECTORY DataDirectory2;
		[FieldOffset(136)] public IMAGE_DATA_DIRECTORY DataDirectory3;
		[FieldOffset(144)] public IMAGE_DATA_DIRECTORY DataDirectory4;
		[FieldOffset(152)] public IMAGE_DATA_DIRECTORY DataDirectory5;
		[FieldOffset(160)] public IMAGE_DATA_DIRECTORY DataDirectory6;
		[FieldOffset(168)] public IMAGE_DATA_DIRECTORY DataDirectory7;
		[FieldOffset(176)] public IMAGE_DATA_DIRECTORY DataDirectory8;
		[FieldOffset(184)] public IMAGE_DATA_DIRECTORY DataDirectory9;
		[FieldOffset(192)] public IMAGE_DATA_DIRECTORY DataDirectory10;
		[FieldOffset(200)] public IMAGE_DATA_DIRECTORY DataDirectory11;
		[FieldOffset(208)] public IMAGE_DATA_DIRECTORY DataDirectory12;
		[FieldOffset(216)] public IMAGE_DATA_DIRECTORY DataDirectory13;
		[FieldOffset(224)] public IMAGE_DATA_DIRECTORY DataDirectory14;
		[FieldOffset(232)] public IMAGE_DATA_DIRECTORY DataDirectory15;

		public static unsafe IMAGE_DATA_DIRECTORY* GetDataDirectory(IMAGE_OPTIONAL_HEADER64* header, int zeroBasedIndex)
		{
			return &header->DataDirectory0 + zeroBasedIndex;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGE_DATA_DIRECTORY
	{
		public uint VirtualAddress;
		public uint Size;
	}

	/// <summary>
	///     Describes a symbol within a module.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_MODULE_AND_ID
	{
		/// <summary>
		///     The location in the target's virtual address space of the module's base address.
		/// </summary>
		public ulong ModuleBase;

		/// <summary>
		///     The symbol ID of the symbol within the module.
		/// </summary>
		public ulong Id;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_SYMBOL_ENTRY
	{
		public ulong ModuleBase;
		public ulong Offset;
		public ulong Id;
		public ulong Arg64;
		public uint Size;
		public uint Flags;
		public uint TypeId;
		public uint NameSize;
		public uint Token;
		public SymTag Tag;
		public uint Arg32;
		public uint Reserved;
	}


	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_IMPORT_DESCRIPTOR
	{
		[FieldOffset(0)] public uint Characteristics; // 0 for terminating null import descriptor
		[FieldOffset(0)] public uint OriginalFirstThunk; // RVA to original unbound IAT (PIMAGE_THUNK_DATA)
		[FieldOffset(4)] public uint TimeDateStamp; // 0 if not bound,
		// -1 if bound, and real date\time stamp
		//     in IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT (new BIND)
		// O.W. date/time stamp of DLL bound to (Old BIND)

		[FieldOffset(8)] public uint ForwarderChain; // -1 if no forwarders
		[FieldOffset(12)] public uint Name;
		[FieldOffset(16)] public uint FirstThunk; // RVA to IAT (if bound this IAT has actual addresses)
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_THUNK_DATA32
	{
		[FieldOffset(0)] public uint ForwarderString; // PBYTE
		[FieldOffset(0)] public uint Function; // PDWORD
		[FieldOffset(0)] public uint Ordinal;
		[FieldOffset(0)] public uint AddressOfData; // PIMAGE_IMPORT_BY_NAME
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_THUNK_DATA64
	{
		[FieldOffset(0)] public ulong ForwarderString; // PBYTE
		[FieldOffset(0)] public ulong Function; // PDWORD
		[FieldOffset(0)] public ulong Ordinal;
		[FieldOffset(0)] public ulong AddressOfData; // PIMAGE_IMPORT_BY_NAME
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct LANGANDCODEPAGE
	{
		[FieldOffset(0)] public ushort wLanguage;
		[FieldOffset(2)] public ushort wCodePage;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct VS_FIXEDFILEINFO
	{
		public uint dwSignature;
		public uint dwStrucVersion;
		public uint dwFileVersionMS;
		public uint dwFileVersionLS;
		public uint dwProductVersionMS;
		public uint dwProductVersionLS;
		public uint dwFileFlagsMask;
		public VS_FF dwFileFlags;
		public uint dwFileOS;
		public uint dwFileType;
		public uint dwFileSubtype;
		public uint dwFileDateMS;
		public uint dwFileDateLS;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct IMAGE_COR20_HEADER_ENTRYPOINT
	{
		[FieldOffset(0)] private readonly uint _token;
		[FieldOffset(0)] private readonly uint _RVA;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGE_COR20_HEADER
	{
		// Header versioning
		public uint cb;
		public ushort MajorRuntimeVersion;
		public ushort MinorRuntimeVersion;

		// Symbol table and startup information
		public IMAGE_DATA_DIRECTORY MetaData;
		public uint Flags;

		// The main program if it is an EXE (not used if a DLL?)
		// If COMIMAGE_FLAGS_NATIVE_ENTRYPOINT is not set, EntryPointToken represents a managed entrypoint.
		// If COMIMAGE_FLAGS_NATIVE_ENTRYPOINT is set, EntryPointRVA represents an RVA to a native entrypoint
		// (depricated for DLLs, use modules constructors intead).
		public IMAGE_COR20_HEADER_ENTRYPOINT EntryPoint;

		// This is the blob of managed resources. Fetched using code:AssemblyNative.GetResource and
		// code:PEFile.GetResource and accessible from managed code from
		// System.Assembly.GetManifestResourceStream.  The meta data has a table that maps names to offsets into
		// this blob, so logically the blob is a set of resources.
		public IMAGE_DATA_DIRECTORY Resources;
		// IL assemblies can be signed with a public-private key to validate who created it.  The signature goes
		// here if this feature is used.
		public IMAGE_DATA_DIRECTORY StrongNameSignature;

		public IMAGE_DATA_DIRECTORY CodeManagerTable; // Depricated, not used
		// Used for manged codee that has unmaanaged code inside it (or exports methods as unmanaged entry points)
		public IMAGE_DATA_DIRECTORY VTableFixups;
		public IMAGE_DATA_DIRECTORY ExportAddressTableJumps;

		// null for ordinary IL images.  NGEN images it points at a code:CORCOMPILE_HEADER structure
		public IMAGE_DATA_DIRECTORY ManagedNativeHeader;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct WDBGEXTS_THREAD_OS_INFO
	{
		public uint ThreadId;
		public uint ExitStatus;
		public uint PriorityClass;
		public uint Priority;
		public ulong CreateTime;
		public ulong ExitTime;
		public ulong KernelTime;
		public ulong UserTime;
		public ulong StartOffset;
		public ulong Affinity;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct WDBGEXTS_CLR_DATA_INTERFACE
	{
		public Guid* Iid;
		private readonly void* _iface;

		public WDBGEXTS_CLR_DATA_INTERFACE(Guid* iid)
		{
			Iid = iid;
			_iface = null;
		}

		public object Interface
		{
			get { return _iface != null ? Marshal.GetObjectForIUnknown((IntPtr) _iface) : null; }
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_SYMBOL_SOURCE_ENTRY
	{
		private readonly ulong _moduleBase;
		private readonly ulong _offset;
		private readonly ulong _fileNameId;
		private readonly ulong _engineInternal;
		private readonly uint _size;
		private readonly uint _flags;
		private readonly uint _fileNameSize;
		// Line numbers are one-based.
		// May be DEBUG_ANY_ID if unknown.
		private readonly uint _startLine;
		private readonly uint _endLine;
		// Column numbers are one-based byte indices.
		// May be DEBUG_ANY_ID if unknown.
		private readonly uint _startColumn;
		private readonly uint _endColumn;
		private readonly uint _reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_OFFSET_REGION
	{
		private readonly ulong _base;
		private readonly ulong _size;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct _DEBUG_TYPED_DATA
	{
		public ulong ModBase;
		public ulong Offset;
		public ulong EngineHandle;
		public ulong Data;
		public uint Size;
		public uint Flags;
		public uint TypeId;
		public uint BaseTypeId;
		public uint Tag;
		public uint Register;
		public fixed ulong Internal [9];
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct _EXT_TYPED_DATA
	{
		public _EXT_TDOP Operation;
		public uint Flags;
		public _DEBUG_TYPED_DATA InData;
		public _DEBUG_TYPED_DATA OutData;
		public uint InStrIndex;
		public uint In32;
		public uint Out32;
		public ulong In64;
		public ulong Out64;
		public uint StrBufferIndex;
		public uint StrBufferChars;
		public uint StrCharsNeeded;
		public uint DataBufferIndex;
		public uint DataBufferBytes;
		public uint DataBytesNeeded;
		public uint Status;
		public fixed ulong Reserved [8];
	}


	[StructLayout(LayoutKind.Sequential)]
	public class EXT_TYPED_DATA
	{
		public uint DataBufferBytes;
		public uint DataBufferIndex;
		public uint DataBytesNeeded;
		public uint Flags;
		public uint In32;
		public ulong In64;
		public _DEBUG_TYPED_DATA InData;
		public uint InStrIndex;
		public _EXT_TDOP Operation;
		public uint Out32;
		public ulong Out64;
		public _DEBUG_TYPED_DATA OutData;
		public uint Status;
		public uint StrBufferChars;
		public uint StrBufferIndex;
		public uint StrCharsNeeded;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		public int left;
		public int top;
		public int right;
		public int bottom;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_LAST_EVENT_INFO_BREAKPOINT
	{
		public uint Id;
	}


	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_LAST_EVENT_INFO_EXCEPTION
	{
		public EXCEPTION_RECORD64 ExceptionRecord;
		public uint FirstChance;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_LAST_EVENT_INFO_EXIT_THREAD
	{
		public uint ExitCode;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_LAST_EVENT_INFO_EXIT_PROCESS
	{
		public uint ExitCode;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_LAST_EVENT_INFO_LOAD_MODULE
	{
		public ulong Base;
	}


	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_LAST_EVENT_INFO_UNLOAD_MODULE
	{
		public ulong Base;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_LAST_EVENT_INFO_SYSTEM_ERROR
	{
		public uint Error;
		public uint Level;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct DEBUG_LAST_EVENT_INFO
	{
		[FieldOffset(0)] public DEBUG_LAST_EVENT_INFO_BREAKPOINT Breakpoint;
		[FieldOffset(0)] public DEBUG_LAST_EVENT_INFO_EXCEPTION Exception;
		[FieldOffset(0)] public DEBUG_LAST_EVENT_INFO_EXIT_THREAD ExitThread;
		[FieldOffset(0)] public DEBUG_LAST_EVENT_INFO_EXIT_PROCESS ExitProcess;
		[FieldOffset(0)] public DEBUG_LAST_EVENT_INFO_LOAD_MODULE LoadModule;
		[FieldOffset(0)] public DEBUG_LAST_EVENT_INFO_UNLOAD_MODULE UnloadModule;
		[FieldOffset(0)] public DEBUG_LAST_EVENT_INFO_SYSTEM_ERROR SystemError;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DEBUG_EVENT_CONTEXT
	{
		public uint Size;
		public uint ProcessEngineId;
		public uint ThreadEngineId;
		public uint FrameEngineId;
	}
}