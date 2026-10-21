# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

The solution is built through the MSBuild traversal project `dirs.proj` (SDK pinned in `global.json`, currently .NET 10 / C# 14). On non-Windows hosts `WoWsShipBuilder.Desktop.Test` is skipped automatically.

```shell
dotnet restore dirs.proj
dotnet build dirs.proj -c Release -p:TreatWarningsAsErrors=true   # what CI runs; warnings fail the build
dotnet test dirs.proj --no-build -c Release

# single test project / single test (NUnit)
dotnet test tests/WoWsShipBuilder.Common.Test
dotnet test tests/WoWsShipBuilder.Common.Test --filter "FullyQualifiedName~BuildDeserialization"
dotnet test tests/WoWsShipBuilder.Common.Test --filter "Name=MethodName_Precondition_Postcondition"

# run the apps
dotnet run --project src/WoWsShipBuilder.Web       # Blazor Server
dotnet run --project src/WoWsShipBuilder.Desktop   # Windows only (Avalonia + WebView2)

# local desktop release test (requires the vpk tool matching the Velopack package version)
build/BuildVelopackRelease.ps1
```

There is no separate lint step: StyleCop.Analyzers, the .NET analyzers (`AnalysisMode=Recommended`), nullable reference types and the custom data element analyzer all run as part of the build. Fix warnings rather than suppressing them.

## Architecture

### Projects

- **WoWsShipBuilder.Common** (Razor class library): almost all UI and logic. Blazor components (MudBlazor), ReactiveUI viewmodels, data containers, localization. Shared by both hosts.
- **WoWsShipBuilder.Web**: ASP.NET Core Blazor Server host (interactive server render mode). Loads game data once at startup (`DataInitializer`), adds auth, metrics (Prometheus/OpenTelemetry), Docker build. Version via Nerdbank.GitVersioning (`version.json`).
- **WoWsShipBuilder.Desktop**: Avalonia window hosting a Blazor `BlazorWebView` (WindowsForms/WebView2), so it renders the same Common components. Adds Velopack auto-update, local game data caching and updating (`Features/Updater/LocalDataUpdater`), NLog + Sentry. Version via `<VersionPrefix>` in the csproj. Sets `AppData.WebMode = false`.
- **WoWsShipBuilder.DataElements**: small library of `IDataElement` types (key/value, key/value/unit, tooltip, grouped, formatted text) used to render stat panels generically.
- **WoWsShipBuilder.Data.Generator**: Roslyn incremental source generators + analyzer (netstandard2.0), referenced as an analyzer by Common/Desktop:
  - `DataElementGenerator`: for classes marked `[DataContainer]` (deriving from `DataContainerBase`), generates `UpdateDataElements()` which turns `[DataElementType(...)]`-annotated properties into `DataElements`. Containers call `this.UpdateDataElements()` after their properties are set. `DataElementAnalyzer` validates attribute usage at compile time.
  - `PropertyChangedSourceGenerator`: for `partial` `ReactiveObject` classes, generates ReactiveUI properties from private fields annotated with `[Observable]` (optional setter visibility and `Dependants`).

The game data model types (`Ship`, `Projectile`, `Consumable`, …) come from the external NuGet package `WoWsShipBuilder.DataStructures`. The game data itself is produced by a separate data converter project and served from a CDN. The client checks the major/minor version of the data structures schema for compatibility.

### Game data and per-host services

`AppData` (Common) is a static runtime store for loaded game data (ship dictionary, projectile/aircraft/captain/exterior caches, …). Each host provides its own implementations of `IAppDataService`, `IUserDataService`, `ISettingsAccessor`, `IClipboardService` and `IAwsClient`: `ServerAppDataService`/`ServerAwsClient` on Web, `DesktopAppDataService`/`DesktopDataService`/`AwsClient` on Desktop. Shared registrations live in `Common/Infrastructure/Utility/ServiceCollectionExtensions.UseShipBuilder()`; each host has its own setup extension (`UseShipBuilderWeb`, `UseShipBuilderDesktop`).

### Ship stats flow

Code in Common is organized by feature under `Features/` (ShipStats, BallisticCharts, ShipComparison, Builds, DataContainers, …) and shared infrastructure under `Infrastructure/`.

- `ShipViewModel` owns the per-ship build component viewmodels (modules, upgrades, captain skills, signals, consumables), all implementing `IBuildComponent`/`IBuildComponentProvider` for loading from and saving to a `Build`.
- When a component changes, the selected configuration and the collected modifiers are passed to `ShipDataContainer.CreateFromShip(...)`, which builds the tree of `*DataContainer`s (in `Features/DataContainers`). The UI then renders their generated data elements.
- `VmCache` keeps the viewmodels of open ships alive across navigation and disposes them on removal.

### Builds

Builds are shared as build strings. **Deserialization of every historical build string format (v1–v4) must remain supported.** Generating old formats may be obsoleted, but parsing them never may. See `docs/BuildSpec.md`.

### Localization

UI strings live in `Common/Infrastructure/Localization/Resources/Translation.resx`. Only edit the English `Translation.resx`; the other `Translation.*.resx` files are managed through Crowdin (`crowdin.yml`). Game-data strings are loaded at runtime by `LocalizationProvider`.

## Conventions

- Code style is enforced by StyleCop (`stylecop.json`, `.editorconfig`): `this.` qualification, camelCase private fields without underscore prefix, `using` directives outside namespaces, StyleCop element ordering, XML docs on public members. Soft line limit of 160 characters.
- Unit test methods are named `TestedMethodName_Precondition_Postcondition`. Tests use NUnit, FluentAssertions and Moq, with `System.IO.Abstractions` test helpers for file access.
- New NuGet dependencies must be cleared with maintainers and must work in a trimmed release.

## Git workflow

- Do not modify any git settings (config, hooks, remotes, etc.) unless explicitly told to.
- Do not commit automatically. Every change has to be reviewed by the developer first, so finish by suggesting a commit message instead. Commit only when explicitly asked to.
- Commit message format:
  - Summary line in imperative mood, starting with a lowercase verb (e.g. "fix disposal of ship viewmodel").
  - Add a description body only if it provides valuable information beyond the summary. Separate it from the summary with a blank line.
  - No emojis. Reference issues with `#ID` / `fixes #ID`.

## Branches and releases

`development` is the main branch. Releases are PRs from `development` into `release/desktop` and `release/web`, merged with a **merge commit only**. Before a release, bump `<VersionPrefix>` in `src/WoWsShipBuilder.Desktop/WoWsShipBuilder.Desktop.csproj` and `version` in `src/WoWsShipBuilder.Web/version.json`, and update `docs/ReleaseNotes.md`. The release notes become the GitHub release body. Full process: `docs/RELEASE.md`.
