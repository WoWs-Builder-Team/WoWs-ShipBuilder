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
Tools\Squirrel.com --releasify (Get-Item -Path WoWsShipBuilder.UI\bin\$buildConfig\WoWsShipBuilder.*.nupkg) --selfContained --releaseDir=".\releases" --setupIcon="WoWsShipBuilder.UI\Assets\ShipBuilderIcon_bg.ico" --icon="WoWsShipBuilder.UI\Assets\ShipBuilderIcon_bg.ico" --no-delta --no-msi --loadingGif="LoadingIcon.gif"
Write-Output "Local release test completed"