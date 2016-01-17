$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path).Replace(".Tests.", ".")
. "$here\$sut"

$simpleCharFields =  @'
struct _PEB, 105 elements, 0x7a0 bytes
   +0x000 InheritedAddressSpace : UChar
   +0x001 ReadImageFileExecOptions : UChar
   +0x002 BeingDebugged    : UChar
'@  -split "`r`n"

 $bitFields =  @'
struct _PEB, 105 elements, 0x7a0 bytes   
   +0x000 BitField         : UChar
   +0x000 ImageUsesLargePages : Bitfield Pos 0, 1 Bit
   +0x000 IsProtectedProcess : Bitfield Pos 1, 3 Bit
   +0x000 IsImageDynamicallyRelocated : Bitfield Pos 4, 1 Bit
   +0x000 SkipPatchingUser32Forwarders : Bitfield Pos 5, 1 Bit
   
'@  -split "`r`n"

 $bitFields2 =  @'
struct _PEB, 1 elements, 0x1 bytes   
   +0x000 Offset           : Bitfield Pos 0, 31 Bits
   +0x000 HasRenderingCommand : Bitfield Pos 31, 1 Bit   
'@  -split "`r`n"

$bitFields3 =  @'
struct _PEB, 105 elements, 0x7a0 bytes   
   +0x000 BitField         : UChar
   +0x004 Offset           : Bitfield Pos 0, 31 Bits
   +0x004 HasRenderingCommand : Bitfield Pos 31, 1 Bit   
'@  -split "`r`n"

$structField =  @'
struct _ACTIVATION_CONTEXT_STACK, 5 elements, 0x20 bytes  
   +0x000 FrameListCache   : struct _LIST_ENTRY, 2 elements, 0x10 bytes   
'@  -split "`r`n"

$podArray =  @'
struct _GDI_TEB_BATCH, 4 elements, 0x4e8 bytes
   +0x000 Offset           : Bitfield Pos 0, 31 Bits
   +0x000 HasRenderingCommand : Bitfield Pos 31, 1 Bit
   +0x008 HDC              : Uint8B
   +0x010 Buffer           : [310] Uint4B
'@  -split "`r`n"
 
$structArray =  @'
struct _RTL_USER_PROCESS_PARAMETERS, 33 elements, 0x410 bytes
   +0x000 CurrentDirectores : [32] struct _RTL_DRIVE_LETTER_CURDIR, 4 elements, 0x18 bytes
'@  -split "`r`n"

$Ptr =  @'
struct _CLIENT_ID, 2 elements, 0x10 bytes
   +0x000 UniqueProcess    : Ptr64 to Void
   +0x008 UniqueThread     : Ptr64 to Void
'@  -split "`r`n"

$PtrToPtr =  @'
struct _PEB, 4 elements, 0x18 bytes
   +0x000 InheritedAddressSpace : UChar
   +0x007 Padding : [7] UChar
   +0x008 ReadOnlyStaticServerData : Ptr64 to Ptr64 to Void
   +0x010 Ldr              : Ptr64 to struct _PEB_LDR_DATA, 9 elements, 0x58 bytes
'@  -split "`r`n"
 
Describe “Can parse structs" {
    It "Simple Char fields" {
        
        $res = Import-StructFile -Lines $simpleCharFields
        $res.Fields.Count | Should Be 3
        $first = $res.Fields[0]
        $first.Name | Should Be 'InheritedAddressSpace'
        $first.Type.TypeName | Should Be 'UChar'
        $first.Offset | Should Be 0
    }

    It "Parses UChar bitfields" {
       
        $res = Import-StructFile -Lines $bitFields
        $res.Fields.Count | Should Be 1
        $bf = $res.Fields[0]
        $bf.Name | Should Be 'BitField'
        $bf.Type.TypeName | Should Be 'UChar'
        $bf.Offset | Should Be 0

        $secBit = $bf.TYpe.BitFields[1]
        $secBit.Name | Should Be 'IsProtectedProcess'
        $secBit.Position | Should Be 1
        $secBit.Bits | Should Be 3
    }

    It "Parses Uint4B bitfields as first field" {
       
        $res = Import-StructFile -Lines $bitFields2
        $res.Fields.Count | Should Be 1
        $bf = $res.Fields[0]
        $bf.Name | Should Be 'BitField'
        $bf.Type.TypeName | Should Be 'Uint4B'
        $bf.Offset | Should Be 0

        $secBit = $bf.TYpe.BitFields[1]
        $secBit.Name | Should Be 'HasRenderingCommand'
        $secBit.Position | Should Be 31
        $secBit.Bits | Should Be 1
    }
    
    It "Parses Uint4B bitfields" {
       
        $res = Import-StructFile -Lines $bitFields3
        $res.Fields.Count | Should Be 1
        $bf = $res.Fields[0]
        $bf.Name | Should Be 'BitField'
        $bf.Type.TypeName | Should Be 'UChar'
        $bf.Offset | Should Be 0

        $secBit = $bf.TYpe.BitFields[1]
        $secBit.Name | Should Be 'HasRenderingCommand'
        $secBit.Position | Should Be 31
        $secBit.Bits | Should Be 1
    }

     It "Parses struct fields" {
       
        $res = Import-StructFile -Lines $structField
        $res.Size  | Should Be 32
        $res.Fields.Count | Should Be 1
        $sf = $res.Fields[0]        
        $sf.Name | Should Be 'FrameListCache'
        $sf.Offset | Should Be 0
        $sf.Type.GetType().Name  | Should Be 'StructDbgType'
        $sf.Type.TypeName | Should Be '_LIST_ENTRY'        
        $sf.Type.FieldCount | Should Be 2
    }

    It "Parses podarray fields" {
       
        $res = Import-StructFile -Lines $podArray
        $res.Fields.Count | Should Be 3
        $af = $res.Fields[2]
        $af.Offset | Should Be 0x10
        $af.Name | Should Be 'Buffer'
        $type = $af.Type
        $type.TypeName | Should Be 'Uint4B'
        $type.Rank | Should Be 310        
    }

    It "Parses structarray fields" {
       
        $res = Import-StructFile -Lines $structArray
        $res.Fields.Count | Should Be 1
        $af = $res.Fields[0]
        $af.Offset | Should Be 0
        $af.Name | Should Be 'CurrentDirectores'
        $af.Type.GetType().Name | Should Be ArrayDbgType
        $et = $af.Type
        $et = $et.ElementType
        $et.TypeName | Should Be '_RTL_DRIVE_LETTER_CURDIR'
        $et.FieldCount | Should Be 4     
    }
    
    It "Parses ptr fields" {
       
        $res = Import-StructFile -Lines $Ptr
        $res.Fields.Count | Should Be 2
        $pf = $res.Fields[1]
        $pf.Name | Should Be 'UniqueThread'
        $pf.Offset | Should Be 8
        $pf.Type.Size | Should Be 8
        $pt = $pf.Type.Pointee
        $pT.Size | Should Be 0
        $pt.TypeName | Should Be 'Void'        
    }

    It "Parses ptr ptr fields" {
       
        $res = Import-StructFile -Lines $PtrToPtr
        $res.Fields.Count | Should Be 4
        $f = $res.Fields[2]
        $f.Name | Should Be 'ReadOnlyStaticServerData'
        $f.Offset | Should Be 8
        $p = $f.Type.Pointee
        $p.GetType().Name | Should Be PointerDbgType
        $p.TypeName | Should Be 'Ptr64 to Void'
        $p.Pointee.TypeName | Should Be 'Void'
           
    }
}
