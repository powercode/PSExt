@{

# Script module or binary module file associated with this manifest.
RootModule = 'psext.dll'

# Version number of this module.
ModuleVersion = '1.0.0'

# ID used to uniquely identify this module
GUID = '9ca1ec3e-d12b-41e8-9ec5-8abdc592b3bf'

# Author of this module
Author = 'sgustafsson'

# Company or vendor of this module
CompanyName = 'PowerCode'

# Description of the functionality provided by this module
Description = 'Structured debugger output'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '5.0'

RequiredAssemblies = @('.\psext.dll')

FormatsToProcess=@('psext.format.ps1xml')
TypesToProcess=@('psext.types.ps1xml')

# Functions to export from this module
FunctionsToExport = '*'

# Cmdlets to export from this module
CmdletsToExport = '*'

# Variables to export from this module
VariablesToExport = '*'

# Aliases to export from this module
AliasesToExport = '*'


# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = @('debug')

        # A URL to the license for this module.
        # LicenseUri = ''

        # A URL to the main website for this project.
        ProjectUri = 'http://github.com/powercode/psext'

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        ReleaseNotes = @'
'@

    } # End of PSData hashtable

} # End of PrivateData hashtable

}

