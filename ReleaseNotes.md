# Update 1.5.1

## Additions
- Add ability to change plot plane for vertical dispersion chart.
- Allow to include consumable effects in ship statistics. (#141)
- Add additional parameters for depth charges.
- Add damage distribution chart for depth charges.
- Add HP and amount of repairable damage parameters for each ship section.
- Add ships nation flags.

## Changes
- The gamedata updater no longer downloads data that is not supported by the current application version. 
- Engine power timings now take into account the game time scaling.
- Remove server side reload time, rate of fire and DPM parameters because the server tick rate issue has been fixed.

## Bugfixes
- Fix "Skills & Talents" button not enabling when loading a build with conditional skills selected.
- Fix torpedo reload time parameter being displayed incorrectly in the torpedo reload booster consumable.
- Fix tier 1 ships names sometimes being displayed incorrectly.

## Known Issues
- Hull A only consumables are still displayed as available when selecting hull B on some ships.
___
To download the program for the first time, you need to download only the Setup.exe file. The other files are used by
the application for automatic updates.
