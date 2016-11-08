param([Parameter(Mandatory = $true)] $version)

Write-Host "Updating version to $version"

$project = "src\RoslynSerializer\project.json"
$content = Get-Content $project
$newcontent = $content.Replace("`"version`": `"1.0.0-*`"","`"version`": `"$version`"")
Set-Content $project $newcontent

$project = "test\RoslynSerializer.Test\project.json"
$content = Get-Content $project
$newcontent = $content.Replace("`"RoslynSerializer`": `"1.0.0-*`"","`"RoslynSerializer`": `"$version`"")
Set-Content $project $newcontent