using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class SecondaryGridDataWrapper
{
    public SecondaryGridDataWrapper(IReadOnlyCollection<SecondaryBatteryDataContainer>? secondaryBattery)
    {
        // Secondaries
        this.Caliber = secondaryBattery?.Select(x => x.GunCaliber).ToList() ?? new();
        this.BarrelCount = secondaryBattery?.Select(x => x.BarrelsCount).ToList() ?? new();
        this.BarrelsLayout = secondaryBattery?.Select(x => x.BarrelsLayout).ToList() ?? new();
        this.Range = secondaryBattery?.Select(x => x.Range).First();
        this.Reload = secondaryBattery?.Select(x => x.Reload).ToList() ?? new();
        this.RoF = secondaryBattery?.Select(x => x.RoF).ToList() ?? new();
        this.Dpm = secondaryBattery?.Select(x => x.TheoreticalDpm).ToList() ?? new();
        this.Fpm = secondaryBattery?.Select(x => x.PotentialFpm).ToList() ?? new();
        this.Sigma = secondaryBattery?.Select(x => x.Sigma).First();

        // Secondary shells
        List<ShellDataContainer?>? secondaryShellData = secondaryBattery?.Select(x => x.Shell).ToList();

        this.Type = secondaryShellData?.Select(x => x?.Type).First();
        this.Mass = secondaryShellData?.Select(x => x?.Mass ?? 0).ToList() ?? new();
        this.Damage = secondaryShellData?.Select(x => x?.Damage ?? 0).ToList() ?? new();
        this.SplashRadius = secondaryShellData?.Select(x => x?.SplashRadius ?? 0).ToList() ?? new();
        this.SplashDamage = secondaryShellData?.Select(x => x?.SplashDmg ?? 0).ToList() ?? new();
        this.Penetration = secondaryShellData?.Select(x => x?.Penetration ?? 0).ToList() ?? new();
        this.Speed = secondaryShellData?.Select(x => x?.ShellVelocity ?? 0).ToList() ?? new();
        this.AirDrag = secondaryShellData?.Select(x => x?.AirDrag ?? 0).ToList() ?? new();
        this.HeShellFireChance = secondaryShellData?.Select(x => x?.ShellFireChance ?? 0).ToList() ?? new();
        this.HeBlastRadius = secondaryShellData?.Select(x => x?.ExplosionRadius ?? 0).ToList() ?? new();
        this.HeBlastPenetration = secondaryShellData?.Select(x => x?.SplashCoeff ?? 0).ToList() ?? new();
        this.SapOvermatch = secondaryShellData?.Select(x => x?.Overmatch ?? 0).ToList() ?? new();
        this.SapRicochet = secondaryShellData?.Select(x => x?.RicochetAngles ?? default!).ToList() ?? new();
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

    // Secondary shells
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
