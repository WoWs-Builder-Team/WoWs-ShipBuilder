param(
    [string]$buildConfig="Release"
)

Write-Output "Building application"
dotnet build -c $buildConfig
Write-Output "Publishing build"
dotnet publish WoWsShipBuilder.UI -c $buildConfig -p:PublishProfile=PublishWindows
Copy-Item -Path WoWsShipBuilder.UI\bin\$buildConfig\net5.0\runtimes\win7-x64\native\av_libGLESv2.dll -Destination WoWsShipBuilder.UI\bin\$buildConfig\net5.0\publish\
Write-Output "Packing published build into nuget package"
dotnet pack WoWsShipBuilder.UI -c $buildConfig --no-build
Write-Output "Creating Squirrel.Windows release"
$packageLocation = Get-Item -Path WoWsShipBuilder.UI\bin\$buildConfig\WoWsShipBuilder.*.nupkg
Tools\Squirrel.exe releasify --package=$packageLocation --releaseDir=".\releases" --setupIcon="WoWsShipBuilder.UI\Assets\ShipBuilderIcon_bg.ico" --updateIcon="WoWsShipBuilder.UI\Assets\ShipBuilderIcon_bg.ico" --no-delta --splashImage="LoadingIcon.gif"
Write-Output "Local release test completed"