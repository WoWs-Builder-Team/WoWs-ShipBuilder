# Update 1.3.0 - pre1

## Additions
- Add button to open dispersion and ballistics window directly from start menu
- Add dispersion plot in the dispersion and ballistics window
- Add optional telemetry data in settings. Telemetry data is disabled by default and needs to be enabled manually
- Add a tooltip for dispersion formula in main battery column (#10)

## Changes
- Windows now automatically resize to fit on smaller screens or on screens with dpi scaling enabled (#56)

## Bugfixes
- Show missing modules on aircraft carriers (#38)
- Fixed a crash when selecting ship with no main guns in the dispersion or ballistic graphs
- Fixed a display bug of Montana's turret angles
- Fixed invalid app manifest configurations regarding scaling
- Hide AA expander when there is no anti air on a ship (#45)
- Fix InvalidOperationException when showing the dialog after an application update (#46)
- Clear list of ships for the ship selection window after the server type is changed in the settings window
- Update embedded icons to show correct versions of the Airstrike upgrade
- Fixed modifier calculation for some CV upgrades
- Fixed additional spotter planes provided by "Eye in the Sky" not being taken into account
- Fixed engine upgrade modifier calculation
- Fixed bug in incremental image updates using the wrong version name
- Fixed wrong unit for RoF in ship stats
___
To download the program for the first time, you need to download only the Setup.exe file. The other files are used by the application for automatic updates.

