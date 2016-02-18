using namespace System.Collections.Generic

Set-StrictMode -Version Latest

class Util
{
    static [Hashtable] $namemap = @{
        UChar = 'byte'
        Char = 'byte'
        WChar = 'short'
        Int2B = 'short'
        Int4B = 'int'
        Int8B = 'long'
        UInt2B = 'ushort'
        UInt4B = 'uint'
        UInt8B = 'ulong'
        _LARGE_INTEGER='long'
        _ULARGE_INTEGER='ulong'
    }
    static [string] ToClrName([string] $name)
    {        
        $res = [Util]::namemap[$name]
        if ($res)
        {
            return  $res
        }

        switch -regex ($name)
        {                       
            'Void' {return 'void'}                        
            'struct (.+)' {return $matches.1}
            'Guid' {return 'Guid'}            
            '_M128' {return 'F128PARTS64'}            
        }
        if($name -like '_*')
        {
            return $name.Substring(1)
        }
        return 'IntPtr'
    }

    static [DbgType] ParseType([string] $type)
    {    
        switch -Regex ($type) 
        {
            <#case#>  '^(?<name>.?int(?<size>\d+)B)' {
                return [DbgType]::new($matches.name, $matches.size)                
           }
           <#case#>  '^U?Char'{
                return [DbgType]::new($matches.0, 1)
           }
           <#case#>  'wchar'{
                return [DbgType]::new('Wchar', 2)
           }
          
           <#case#>  '^Void'{
                return [DbgType]::new('Void', 0)
           }        
              
           <#case#>  'Ptr\d+ to \s*(?<type>.*)'{
                $pointee = [Util]::ParseType($matches.type)
                return [PointerDbgType]::new($pointee)
           }

           <#case#>  '\[(?<rank>\d+)\] (?<type>.*)'{
                $elemType = [Util]::ParseType($matches.type)
                return [ArrayDbgType]::new($matches.rank, $elemType)
           }                     

            <#case#>  '(?:union|struct) (?<name>[^,]+), (?<elem>\d+) elements, (?<bytes>\S+) bytes'{
                return [StructDbgType]::new($matches.name, $matches.elem, $matches.bytes)            
            }
            default{                
                return [DbgType]::new($_, 0)
            }                      
        }
        throw "Unable to parse type '$type'" 
    }  
}

class DbgType
{
    $TypeName
    $Size
    $ISPod = $true

    DbgType([string] $name, [int] $size)
    {
        $this.TypeName = $name
        $this.Size = $size
    }

    [string] ToString()
    {
        return $this.TypeName
    }
    
}

class PointerDbgType : DbgType
{
    [DbgType] $Pointee    
    PointerDbgType([DbgType] $Pointee) : Base("Ptr64 to " + $pointee.TypeName, 8)
    {
        $this.Pointee = $Pointee
        $this.ISPod = $false
    }
}

class ArrayDbgType : DbgType
{
    [int] $Rank
    [DbgType] $ElementType

    ArrayDbgType([int] $rank, [DbgType] $elementType) : Base($ElementType.TypeName, $elementtype.Size * $rank){
        $this.Rank = $rank
        $this.ElementType = $ElementType
        $this.ISPod = $false
    }
}

class StructDbgType : DbgType
{
    [int] $FieldCount        

    StructDbgType([string] $name, [string] $fieldCount, [int] $size) : Base($name, $size){        
        $this.FieldCount = $fieldCount
        $this.ISPod = $false
    }
}

class BitField
{    
    [string] $Name
    [int] $Position
    [int] $Bits

    BitField([string] $name, [int] $position, [int] $bits)
    { 
        $this.Name = $name
        $this.Position = $position
        $this.Bits = $bits        
    }

    [string] ToString()
    {
        return "{0} Pos {1} {2} bit(s)" -f $this.Name, $this.Position, $this.Size
    }
}

class BitFieldDbgType : DbgType
{
    [List[BitField]] $BitFields    
    BitFieldDbgType() : Base('Uint4B', 4) {
        $this.BitFields = [List[BitField]]::new()
        $this.ISPod = $true
    }
    BitFieldDbgType([DbgType] $other) : Base($other.TypeName, $other.size){                
        $this.BitFields = [List[BitField]]::new()
        $this.ISPod = $true
    }
}

class FieldInfo
{
    [int]$Offset
    [string] $Name    
    [DbgType] $Type
    
    FieldInfo([int]$Offset, [string] $Name, [DbgType] $type)
    {
        $this.Offset = $Offset
        $this.Name = $Name        
        $this.Type = $type
    }
 
    [string] ToString()
    {        
        return "0x{0:X4} {1} {2}" -f $this.Offset, $this.Type, $this.Name        
    }
}

class Struct{
    [string] $Name
    [int] $ElementCount
    [int] $Size
    [System.Collections.Generic.List[FieldInfo]] $Fields
    [bool] $Unsafe
    Struct([string]$name, [int]$ElementCount,[int]$Size){
        $this.Name = $name
        $this.ElementCount = $ElementCount
        $this.Size = $Size
        $this.Fields = [System.Collections.Generic.List[FieldInfo]]::new()
        $this.Unsafe = $false
    }

    [void] AddField([FieldInfo] $fieldInfo)
    {
        
        $this.Fields.Add($fieldInfo)
    }    
     
}

class CSharpSourceConverter
{
    static [UInt32] MaskBits([int] $position, [int] $count)
    {
        $position = $position
        $end = $position + $count - 1
        return ([UInt32]::MaxValue -shr (31 - $end)) -band (-bnot ((1 -shl ($position)) - 1))
    }
    
    static [string] GetArrayFieldProperties([Struct] $struct, [FieldInfo] $arrayfieldInfo, [bool] $structIsUnsafe)
    {
        $structName = [Util]::ToClrName( $Struct.Name )
        $type = [Util]::ToClrName( $arrayfieldInfo.Type.ElementType.TypeName)
        $name= $arrayfieldInfo.Name   
        $unsafe = if ($structIsUnsafe) {''} else {'unsafe '}      
    return @"
`t`tpublic static ${unsafe}$type* Get$name($structName* item, int zeroBasedIndex)
`t`t{
`t`t`t`treturn &item->${name}0 + zeroBasedIndex;
`t`t}

"@
    }

    static [IList[string]] GetBitFieldProperties([FieldInfo] $bitfieldInfo)
    {
        $fieldName = $bitfieldInfo.Name
        $res = [List[string]]::new()
        $fieldType = [Util]::ToClrName($bitfieldInfo.Type.TypeName)
        foreach($bitField in $bitfieldInfo.Type.BitFields)
        {
            [int]$bits = $bitField.Bits
            [int]$pos = $bitField.Position
            $name = $bitField.Name            
            $mask = [CSharpSourceConverter]::MaskBits($pos,$bits)
            $maskHex = '0x{0:X4}' -f $mask            
            if($bits -eq 1)
            {
                $type = 'bool'            
                $equals = ' == 1'            
                $expr = "($fieldName & $maskHex) >> ${pos} == 1;"
            }    
            else{
                $type = $fieldType
                $equals = ''  
                $cast = ''
                if ($type -ne 'uint')
                {
                    $cast = "($type)"
                }              
                $expr = "$cast(($fieldName & $maskHex) >> ${pos});"
            }        
            
        

            $res.Add("`t`tpublic $type $name => $expr")
        }
        $res.Add("")
        return $res
        
    }

    static [bool] IsUnsafe([Struct] $struct)
    {
        foreach($f in $struct.Fields)
        {
            if ($f.Type -is [ArrayDbgType])
            {
                if($f.Type.ElementType.Size -eq 0)
                {
                    return $true
                }
                if($f.Type.ElementType.IsPod)
                {
                    return $true
                }
            }
            if ($f.Type -is [PointerDbgType])
            {
                if($f.Type.Pointee.IsPod -and $f.Type.Pointee.TypeName -ne 'Void')
                {
                    return $true
                }
            }
        }
        return $false
    }
    static [string] Convert([Struct] $struct)
    {
        $ofs = [Environment]::NewLine
        $structName = $Struct.Name.TrimStart('_')
        $isUnsafe = [CSharpSourceConverter]::IsUnsafe($struct)
        $unsafe = if($isUnsafe){ 'unsafe '} else {''}

         $fields = foreach($f in $struct.Fields)
         {
            $type = [Util]::ToClrName($f.Type.TypeName)
            $name = $f.Name    
            $offset =$f.Offset
            $kind = $f.Type.GetType().Name
            $hexOffset = "0x{0:x3}" -f $Offset
            
            switch ($kind)
            {
                <#case#>'DbgType' {
                    "`t`t[FieldOffset($hexOffset)] public $type $name;"
                   continue
                }
                <#case#>'ArrayDbgType' {
                    $rank = $f.type.Rank
                    $elementSize = $f.Type.ElementType.Size
                    if($f.Type.ElementType.IsPod)                    
                    {
                        "`t`t[FieldOffset($hexOffset)] public fixed $type $name [$rank];"
                    }
                    else
                    {
                        for($i = 0; $i -lt $rank; $i++)
                        {
                            "`t`t[FieldOffset($hexOffset)] public $type ${name}$i;"
                            $offset = $offset + $elementSize
                            $hexOffset = "0x{0:x3}" -f $Offset
                        }
                    }
                    continue
                }
                <#case#>'PointerDbgType' {
                    $pointee = $f.Type.Pointee                    
                    if($Pointee.IsPod -and $pointee.TypeName -ne 'Void')
                    {
                        "`t`t[FieldOffset($hexOffset)] public $type* $name;"
                    }
                    else
                    {
                        "`t`t[FieldOffset($hexOffset)] public IntPtr $name;"
                    }
                    continue

                }
                <#case#>'StructDbgType' {
                    
                        "`t`t[FieldOffset($hexOffset)] public $type $name;"
                    
                    continue

                }
                <#case#>'BitFieldDbgType' {
                    
                    "`t`t[FieldOffset($hexOffset)] public $type $name;"
                    continue
                }
                default {
                    Write-Warning "Missing case for $_"
                }
            }
        }
        $first = $true
        $properties = foreach($f in $struct.Fields)
        {
            if($f.Type -is [BitFieldDbgType]) {
                if($first)
                {
                    $first = $false
                    [Environment]::NewLine
                }
                [CSharpSourceConverter]::GetBitFieldProperties($f)
            }

            
        }

        $properties += foreach($f in $struct.Fields)
        {
            if($f.Type -is [ArrayDbgType] -and !$f.Type.ElementType.IsPod) {
                if($first)
                {
                    $first = $false
                    [Environment]::NewLine
                }
                [CSharpSourceConverter]::GetArrayFieldProperties($struct, $f, $isUnsafe)
            }

            
        }
        $structSize = '0x' + $Struct.Size.Tostring('x')
        return @"

`t/// Generated by SystemStruct.ps1
`t[StructLayout(LayoutKind.Explicit, Size=$structSize)]
`tpublic ${unsafe}struct $structName
`t{
$fields
$properties
`t}

"@
    }
}

function Import-StructFile{
    [CmdletBinding(DefaultParameterSetName='Default')]
    [OutputType([Struct])]
    param(
        [Alias('fullname')]
        [Parameter(ValueFromPipeline, Mandatory, parametersetname = 'Default')]
        [string] $Path,
        [Parameter(ValueFromPipeline, Mandatory, parametersetname = 'Lines')]
        [string[]] $Lines
    )
    process {    
        $current = $null    
        if ($PSCmdlet.ParameterSetName -eq 'Default')
        {
            $Lines = Get-Content $Path
        }
        switch -Regex ($Lines)
        {
            '.*' { $PSCmdlet.WriteDebug($_) }
            <#case#> '^\w' 
            {
                if ($current){
                    $current
                    $current = $null
                }
             }     
            <#case#>  '^struct (?<name>_\w+), (?<no>\d+) elements, (?<hexsize>0x\S+) bytes'
            {
                $current = [Struct]::new($matches.name, $Matches.no, $matches.hexsize)                            
                continue
            }                
            <#case#>  '^\s+\+(?<offset>0x\S+) (?<name>\S+)\s+: (?<type>Bitfield Pos (?<pos>\d+), (?<bits>\d+) Bits?)'
            {
                $bitField = [BitField]::new($matches.name, $matches.Pos, $matches.bits)                        
                if ($current.Fields.Count -eq 0)
                {
                    $field = [FieldInfo]::new($matches.Offset, 'BitField', [BitFieldDbgType]::new())
                    $current.Fields.Add($field)
                }
                else{
                    $field = $current.Fields[-1]
                    if ($field.Type -isnot [BitFieldDbgType])
                    {
                        $field.Type = [BitFieldDbgType]::new($field.Type)                
                    }
                }
                        
                $field.Type.BitFields.Add($bitField)
                continue
            }
            <#case#>  '^\s+\+(?<offset>0x\S+) (?<name>\S+)\s+: (?<type>.+)'
            {
                $type = [Util]::ParseType($matches.type)                                                    
                $field = [FieldInfo]::new($matches.offset, $matches.name, $type)
                $current.Fields.Add($field)                                                           
            }                
        }        
    
        if ($current){
            $current
        }
    }
} 


function ConvertTo-CSharpSrc{
    param(
        [Parameter(ValueFromPipeline, Mandatory)]
        [Struct[]]$Struct
    )
    process{
        foreach($s in $Struct){
            [CSharpSourceConverter]::Convert($s)
        }   
    }     
}

function  Get-DebuggerTypeDumpCommand
{
    param(
        [ValidateSet('X86','X64')]
        [string] $Platform
    )        
    $(
    if($Platform -eq 'X64')
    {    
        '_ACTIVATION_CONTEXT_STACK'
        '_CLIENT_ID'
        '_CURDIR'
        '_EXCEPTION_REGISTRATION_RECORD'
        '_GDI_TEB_BATCH'
        '_LIST_ENTRY'
        '_NT_TIB'
        '_PEB'
        '_PEB_LDR_DATA'
        '_PROCESSOR_NUMBER'
        '_RTL_ACTIVATION_CONTEXT_STACK_FRAME'
        '_RTL_CRITICAL_SECTION'
        '_RTL_CRITICAL_SECTION_DEBUG'
        '_RTL_DRIVE_LETTER_CURDIR'
        '_RTL_USER_PROCESS_PARAMETERS'
        '_STRING'
        '_TEB'
        '_TEB_ACTIVE_FRAME'
        '_TEB_ACTIVE_FRAME_CONTEXT'
        '_UNICODE_STRING'    
        '_CONTEXT'    
        '_XSAVE_FORMAT'    
    }
    else
    {
        '_ACTIVATION_CONTEXT_STACK'
        '_CLIENT_ID'
        '_CURDIR'
        '_EXCEPTION_REGISTRATION_RECORD'
        '_GDI_TEB_BATCH'
        '_LIST_ENTRY'
        '_NT_TIB'
        '_PEB'
        '_PEB_LDR_DATA'
        '_PROCESSOR_NUMBER'
        '_RTL_ACTIVATION_CONTEXT_STACK_FRAME'
        '_RTL_CRITICAL_SECTION'
        '_RTL_CRITICAL_SECTION_DEBUG'
        '_RTL_DRIVE_LETTER_CURDIR'
        '_RTL_USER_PROCESS_PARAMETERS'
        '_STRING'
        '_TEB'
        '_TEB_ACTIVE_FRAME'
        '_TEB_ACTIVE_FRAME_CONTEXT'
        '_UNICODE_STRING'    
        '_CONTEXT'    
        '_XSAVE_FORMAT'
        '_ACTIVATION_CONTEXT_DATA'
        '_ASSEMBLY_STORAGE_MAP'
        '_FLS_CALLBACK_INFO'
        '_FLOATING_SAVE_AREA'
    }
    ) | foreach {
        "dt -v $_"
    }
}


function Get-StructDump
{    
    [OutputType([System.IO.FileInfo])]
    param(
        [ValidateSet('X86','X64')]
        [Parameter(Mandatory, ValueFromPipelineByPropertyName)]
        $Platform,
        [Parameter(Mandatory, ValueFromPipelineByPropertyName)]
        [string] $Exe
    )
    begin{
        
    }
    process{                    
        $cmds = (Get-DebuggerTypeDumpCommand -platform $Platform) -join "`r`n"
        $cmdFile = New-TemporaryFile
        $logFile = "struct_$($Platform)"
        Set-Content -Encoding Ascii -Path $cmdFile -Value @"
.reload -f
.logopen $logFile
$cmds
.logclose
q    
"@
        $null = cdb -y c:\sym -c "`"$<$cmdFile`"" $Exe
        [PSCustomObject] @{
            Result = Get-Item $logFile
            Platform = $Platform
        }
        
    }

}

function ConvertTo-Binary{
    [Alias('ctb')]
    param(
    [Parameter(ValueFromPipeline)]
    [UInt32] $Value
    )
    process{
        [Convert]::ToString($Value,2)
    }
}


function Get-CSharpDebugStruct {
        
    param($PathRoot = "$PSScriptRoot\..\src\Extension\DebugStructs")

    $platforms =@(    
            [PSCustomObject]@{Platform ='x86'; Exe ='C:\windows\SysWOW64\notepad.exe'},
            [PSCustomObject]@{Platform ='x64'; Exe ='C:\windows\notepad.exe'}
        )
    
    $Platforms | Get-StructDump | foreach{
        $p = $_.Platform
        $outFile = "${PathRoot}_$p.cs"
        $s = Import-StructFile -Path $_.Result
        $src = ConvertTo-CSharpSrc -struct $s
        $Namespace = "PSExt.Extension.$p"
@"
using System;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Interop;

// ReSharper disable InconsistentNaming
namespace $Namespace
{

$src

}
"@ | Set-Content -Path $outFile -Encoding Ascii
    Get-Item $outFile

    }
}
