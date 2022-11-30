param(
    [Parameter(Mandatory=$true)]
    [string]
    $projectFile
)

$xml = [Xml] (Get-Content $projectFile)
$versionPrefix = [Version] $xml.Project.PropertyGroup.VersionPrefix
$versionSuffix = $xml.Project.PropertyGroup.VersionSuffix

if ($versionSuffix) {
    $versionSuffix = "-$versionSuffix";
}

return "$versionPrefix$versionSuffix"