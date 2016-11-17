param([Parameter(Mandatory = $true)] $version)

Write-Host "Updating version to $version"

$project = "src\SourceCodeSerializer\project.json"
$content = Get-Content $project
$newcontent = $content.Replace("`"version`": `"1.0.0-*`"","`"version`": `"$version`"")
Set-Content $project $newcontent

$project = "test\SourceCodeSerializer.Test\project.json"
$content = Get-Content $project
$newcontent = $content.Replace("`"SourceCodeSerializer`": `"1.0.0-*`"","`"SourceCodeSerializer`": `"$version`"")
Set-Content $project $newcontent