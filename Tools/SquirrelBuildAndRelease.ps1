param(
    [string]$buildConfig="Release",
    [string][Parameter(Mandatory=$false)]$signingCert,
    [string][Parameter(Mandatory=$false)]$signingPassword
)

$frameworkVersion="net7.0-windows"

Write-Output "Building application"
dotnet build dirs.proj -c $buildConfig

Write-Output "Publishing build"
dotnet publish WoWsShipBuilder.Desktop -c $buildConfig -p:PublishProfile=PublishWindows
$publishDir = "WoWsShipBuilder.Desktop\bin\$buildConfig\publish"

Write-Output "Creating Squirrel.Windows release"
$version = Get-Content -Path WoWsShipBuilder.Desktop\buildInfo.txt
$signingParams = ""
if ($signingCert) {
    Write-Output "Signing release"
    $signingParams = "--signParams=`"/a /f $signingCert /p $signingPassword /fd sha256 /tr http://timestamp.digicert.com /td sha256`""
}
Tools\Squirrel.exe pack --releaseDir=".\releases" --icon="WoWsShipBuilder.Desktop\Assets\ShipBuilderIcon_bg.ico" --appIcon="WoWsShipBuilder.Desktop\Assets\ShipBuilderIcon_bg.ico" --noDelta --splashImage="LoadingIcon.gif" --packId="WoWsShipBuilder" --packVersion="$version" --packDir="$publishDir" --packTitle="WoWsShipBuilder" --includePdb $signingParams


Write-Output "Local release test completed"
