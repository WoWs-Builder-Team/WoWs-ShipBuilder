param(
    [string]$buildConfig="Release",
    [switch]$skipBuild=$true,
    [string][Parameter(Mandatory=$false)]$signingCert,
    [string][Parameter(Mandatory=$false)]$signingPassword
)

if ($skipBuild) {
    Write-Output "Skipping build"
} else {
    Write-Output "Building application"
    dotnet build dirs.proj -c $buildConfig
}

Write-Output "Publishing build"
$publishDir = "publish-desktop"
dotnet publish WoWsShipBuilder.Desktop -c $buildConfig -p:PublishProfile=PublishWindows -o $publishDir

Write-Output "Creating Velopack release"
$version = Get-Content -Path WoWsShipBuilder.Desktop\buildInfo.txt
$signingParams = ""
if ($signingCert) {
    Write-Output "Signing release"
    $absoluteCertificatePath = Resolve-Path $signingCert
    vpk pack -o "releases" --icon "WoWsShipBuilder.Desktop\Assets\ShipBuilderIcon_bg.ico" --splashImage "installer\SplashScreen.gif" --packId "WoWsShipBuilderTest" --packVersion "$version" --packDir "$publishDir" --mainExe "WoWsShipBuilder.exe" --releaseNotes ReleaseNotes.md --signParams "/a /f $absoluteCertificatePath /p $signingPassword /fd sha256 /tr http://timestamp.digicert.com /td sha256"
} else {
    vpk pack -o "releases" --icon "WoWsShipBuilder.Desktop\Assets\ShipBuilderIcon_bg.ico" --splashImage "installer\SplashScreen.gif" --packId "WoWsShipBuilderTest" --packVersion "$version" --packDir "$publishDir" --mainExe "WoWsShipBuilder.exe" --releaseNotes ReleaseNotes.md
}

Write-Output "Velopack build complete"
