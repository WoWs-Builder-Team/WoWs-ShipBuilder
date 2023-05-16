using System;

namespace WoWsShipBuilder.Desktop.ViewModels.Helper;

public static class VersionHelper
{
    public static string StripCommitFromVersion(string rawVersion)
    {
        int commitStartIndex = rawVersion.IndexOf("+", StringComparison.Ordinal);
        if (commitStartIndex > 0)
        {
            rawVersion = rawVersion[..commitStartIndex];
        }

        return rawVersion;
    }
}
