param([Parameter(Mandatory=$true)][string]$releaseVersion)

Write-Output "Setting environment variable for release version"
$env:CURRENT_TAG=$releaseVersion
Write-Output "Building application"
dotnet build -c Release -p:Version="$env:CURRENT_TAG" -p:AssemblyVersion="$env:CURRENT_TAG" -p:FileVersion="$env:CURRENT_TAG"
Write-Output "Publishing build"
dotnet publish WoWsShipBuilder.UI -p:PublishProfile=PublishWindows -p:Version="$env:CURRENT_TAG" -p:AssemblyVersion="$env:CURRENT_TAG" -p:FileVersion="$env:CURRENT_TAG"
Write-Output "Packing published build into nuget package"
dotnet pack WoWsShipBuilder.UI -c Release -p:NuspecFile=WoWsShipBuilder.nuspec -p:NuspecBasePath=bin/Release/net5.0/publish -p:NuspecProperties=version="$env:CURRENT_TAG"
Write-Output "Creating Squirrel.Windows release"
Tools\Squirrel.com --releasify WoWsShipBuilder.UI\bin\Release\WoWsShipBuilder.$env:CURRENT_TAG.nupkg --selfContained --releaseDir=".\releases" --setupIcon="WoWsShipBuilder.UI\Assets\ShipBuilderIcon_bg.ico" --icon="WoWsShipBuilder.UI\Assets\ShipBuilderIcon_bg.ico" --no-delta --no-msi --loadingGif="LoadingIcon.gif"
Write-Output "Local release test completed"