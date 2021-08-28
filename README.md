# WoWs-ShipBuilder

The main repository of the WoWs ShipBuilder project.

## Local repository setup

* The project is based on .NET 5 and requires the corresponding sdk to be installed.
* Configure the NuGet source for the data structure package:
  > dotnet nuget add source --username USER --password TOKEN --store-password-in-clear-text --name github "https://nuget.pkg.github.com/WoWs-Builder-Team/index.json"

  Replace USER with your github username and TOKEN with a github authentication token. Access without authentication is
  not possible right now.
