param([Parameter(Mandatory = $true)] $version)

Write-Host "Updating version to $version"

$props = "$PSScriptRoot\src\SourceCodeSerializer\SourceCodeSerializer.csproj"
[xml]$dirprops = Get-Content $props
$dirprops.Project.PropertyGroup.VersionPrefix = $version
$dirprops.Save($props)
