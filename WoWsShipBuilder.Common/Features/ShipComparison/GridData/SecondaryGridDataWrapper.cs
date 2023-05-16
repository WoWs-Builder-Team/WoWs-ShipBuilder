using SecondaryBatteryDataContainer = WoWsShipBuilder.DataContainers.SecondaryBatteryDataContainer;
using ShellDataContainer = WoWsShipBuilder.DataContainers.ShellDataContainer;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class SecondaryGridDataWrapper
{
    public SecondaryGridDataWrapper(IReadOnlyCollection<SecondaryBatteryDataContainer>? secondaryBattery)
    {
        //Secondaries
        Caliber = secondaryBattery?.Select(x => x.GunCaliber).ToList() ?? new();
        BarrelCount = secondaryBattery?.Select(x => x.BarrelsCount).ToList() ?? new();
        BarrelsLayout = secondaryBattery?.Select(x => x.BarrelsLayout).ToList() ?? new();
        Range = secondaryBattery?.Select(x => x.Range).First();
        Reload = secondaryBattery?.Select(x => x.Reload).ToList() ?? new();
        RoF = secondaryBattery?.Select(x => x.RoF).ToList() ?? new();
        Dpm = secondaryBattery?.Select(x => x.TheoreticalDpm).ToList() ?? new();
        Fpm = secondaryBattery?.Select(x => x.PotentialFpm).ToList() ?? new();
        Sigma = secondaryBattery?.Select(x => x.Sigma).First();

        //Secondary shells
        List<ShellDataContainer?>? secondaryShellData = secondaryBattery?.Select(x => x.Shell).ToList();

        Type = secondaryShellData?.Select(x => x?.Type).First();
        Mass = secondaryShellData?.Select(x => x?.Mass ?? 0).ToList() ?? new();
        Damage = secondaryShellData?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        SplashRadius = secondaryShellData?.Select(x => x?.SplashRadius ?? 0).ToList() ?? new();
        SplashDamage = secondaryShellData?.Select(x => x?.SplashDmg ?? 0).ToList() ?? new();
        Penetration = secondaryShellData?.Select(x => x?.Penetration ?? 0).ToList() ?? new();
        Speed = secondaryShellData?.Select(x => x?.ShellVelocity ?? 0).ToList() ?? new();
        AirDrag = secondaryShellData?.Select(x => x?.AirDrag ?? 0).ToList() ?? new();
        HeShellFireChance = secondaryShellData?.Select(x => x?.ShellFireChance ?? 0).ToList() ?? new();
        HeBlastRadius = secondaryShellData?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        HeBlastPenetration = secondaryShellData?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
        SapOvermatch = secondaryShellData?.Select(x => x?.Overmatch ?? 0).ToList() ?? new();
        SapRicochet = secondaryShellData?.Select(x => x?.RicochetAngles ?? default!).ToList() ?? new();
    }

    public List<decimal> Caliber { get; }

    public List<int> BarrelCount { get; }

    public List<string> BarrelsLayout { get; }

    public decimal? Range { get; }

    public List<decimal> Reload { get; }

    public List<decimal> RoF { get; }

    public List<string> Dpm { get; }

    public List<decimal> Fpm { get; }

    public decimal? Sigma { get; }

    //Secondary shells
    public string? Type { get; }

    public List<decimal> Mass { get; }

    public List<decimal> Damage { get; }

    public List<decimal> SplashRadius { get; }

    public List<decimal> SplashDamage { get; }

    public List<int> Penetration { get; }

    public List<decimal> Speed { get; }

    public List<decimal> AirDrag { get; }

    public List<decimal> HeShellFireChance { get; }

    public List<decimal> HeBlastRadius { get; }

    public List<decimal> HeBlastPenetration { get; }

    public List<decimal> SapOvermatch { get; }

    public List<string> SapRicochet { get; }
}
