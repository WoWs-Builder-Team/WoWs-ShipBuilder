param(
    [string]$buildConfig="Release",
    [switch]$skipBuild=$false,
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
$publishDir = "artifacts/desktop"
dotnet publish "src/WoWsShipBuilder.Desktop" -c $buildConfig -p:PublishProfile=PublishWindows -o $publishDir

Write-Output "Creating Velopack release"
$version = Select-Xml -Path "src/WoWsShipBuilder.Desktop/WoWsShipBuilder.Desktop.csproj" -XPath "//VersionPrefix" | Select-Object -ExpandProperty Node -First 1 | ForEach-Object {$_.InnerXml}
$iconPath = "src/WoWsShipBuilder.Desktop/Assets/ShipBuilderIcon_bg.ico"

if ($signingCert) {
    Write-Output "Signing release"
    $absoluteCertificatePath = Resolve-Path $signingCert
    vpk pack -o "artifacts/releases" --icon "$iconPath" --splashImage "installer/SplashScreen.gif" --packId "WoWsShipBuilder" --packVersion "$version" --packDir "$publishDir" --mainExe "WoWsShipBuilder.exe" --releaseNotes "docs/ReleaseNotes.md" --signParams "/a /f $absoluteCertificatePath /p $signingPassword /fd sha256 /tr http://timestamp.digicert.com /td sha256"
} else {
    vpk pack -o "artifacts/releases" --icon "$iconPath" --splashImage "installer/SplashScreen.gif" --packId "WoWsShipBuilder" --packVersion "$version" --packDir "$publishDir" --mainExe "WoWsShipBuilder.exe" --releaseNotes "docs/ReleaseNotes.md"
}

Write-Output "Velopack build complete"
