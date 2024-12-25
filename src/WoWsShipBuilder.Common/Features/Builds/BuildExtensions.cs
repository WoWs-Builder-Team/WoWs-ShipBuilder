namespace WoWsShipBuilder.Features.Builds;

public static class BuildExtensions
{
    public static void UpgradeBuild(this Build oldBuild)
    {
        Build.UpgradeBuild(oldBuild);
    }
}
