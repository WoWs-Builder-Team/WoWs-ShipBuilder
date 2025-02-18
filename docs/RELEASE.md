# Creating a release

## Major release (both platforms)

Major releases include new features for both platforms.
Major releases always target both target platforms.

- Ensure the correct app version is set in `src/WoWsShipBuilder.Desktop/WoWsShipBuilder.Desktop.csproj` and
  `src/WoWsShipBuilder.Web/version.json`
- Ensure that the `docs/ReleaseNotes.md` file is up-to-date
- Create PRs from `development` to `release/desktop` and `release/web`
    - Title: "Update {version} ({target platform})"
    - Body: A brief and concise description of major changes for reviewers
- Wait for PR approval
- Merge PRs using a merge commit
    - **NEVER USE ANY OTHER MERGE STRATEGY FOR THE PR**

## Minor releases/Bugfixes

Minor releases are usually only bugfix releases.
Minor releases may target one or both of the target platforms.

- Ensure that the `docs/ReleaseNotes.md` file is up-to-date
- **For Desktop:** Ensure the correct app version is set in
  `src/WoWsShipBuilder.Desktop/WoWsShipBuilder.Desktop.csproj`, including the patch and/or build version, e.g. 2025.1.1
  for the first patch release of the 2025.1 release
- **For Web:** Ensure the version in `src/WoWsShipBuilder.Web/version.json` is still set to the last major release (
  Major.Minor, e.g. 2025.1)
- Create a PR from `development` to `release/desktop` or `release/web`
    - Title: "Update {version} ({target platform}) Patch {patch number}"
        - patches are numbered in ascending order per major release, starting from 1
    - Body: A brief and concise description of minor changes for reviewers
- Wait for PR approval
- Merge PRs using a merge commit
    - **NEVER USE ANY OTHER MERGE STRATEGY FOR THE PR**
