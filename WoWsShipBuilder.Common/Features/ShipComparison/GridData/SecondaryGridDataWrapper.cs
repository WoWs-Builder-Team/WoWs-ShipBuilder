using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.Features.DataContainers;

namespace WoWsShipBuilder.Features.ShipComparison.GridData;

public class SecondaryGridDataWrapper
{
    public SecondaryGridDataWrapper(IReadOnlyCollection<SecondaryBatteryDataContainer> secondaryBattery)
    {
        // Secondaries
        this.Caliber = secondaryBattery.Select(x => x.GunCaliber).ToNoSortList();
        this.BarrelCount = secondaryBattery.Select(x => x.BarrelsCount).ToNoSortList();
        this.BarrelsLayout = secondaryBattery.Select(x => x.BarrelsLayout).ToNoSortList();
        this.Range = secondaryBattery.Select(x => x.Range).FirstOrDefault();
        this.Reload = secondaryBattery.Select(x => x.Reload).ToNoSortList();
        this.RoF = secondaryBattery.Select(x => x.RoF).ToNoSortList();
        this.Dpm = secondaryBattery.Select(x => x.TheoreticalDpm).ToNoSortList();
        this.Fpm = secondaryBattery.Select(x => x.PotentialFpm).ToNoSortList();
        this.Sigma = secondaryBattery.Select(x => x.Sigma).FirstOrDefault();
        this.DispersionData = secondaryBattery.Select(x => x.DispersionData).ToList();
        this.DispersionModifier = secondaryBattery.Select(x => x.DispersionModifier).ToList();

        // Secondary shells
        var secondaryShellData = secondaryBattery.Select(x => x.Shell).ToList();

        this.Type = secondaryShellData.Select(x => x?.Type).FirstOrDefault();
        this.Mass = secondaryShellData.Select(x => x?.Mass ?? 0).ToNoSortList();
        this.Damage = secondaryShellData.Select(x => x?.Damage ?? 0).ToNoSortList();
        this.SplashRadius = secondaryShellData.Select(x => x?.SplashRadius ?? 0).ToNoSortList();
        this.SplashDamage = secondaryShellData.Select(x => x?.SplashDmg ?? 0).ToNoSortList();
        this.Penetration = secondaryShellData.Select(x => x?.Penetration ?? 0).ToNoSortList();
        this.Speed = secondaryShellData.Select(x => x?.ShellVelocity ?? 0).ToNoSortList();
        this.AirDrag = secondaryShellData.Select(x => x?.AirDrag ?? 0).ToNoSortList();
        this.HeShellFireChance = secondaryShellData.Select(x => x?.ShellFireChance ?? 0).ToNoSortList();
        this.HeBlastRadius = secondaryShellData.Select(x => x?.ExplosionRadius ?? 0).ToNoSortList();
        this.HeBlastPenetration = secondaryShellData.Select(x => x?.SplashCoeff ?? 0).ToNoSortList();
        this.SapOvermatch = secondaryShellData.Select(x => x?.Overmatch ?? 0).ToNoSortList();
        this.SapRicochet = secondaryShellData.Select(x => x?.RicochetAngles ?? default!).ToNoSortList();
    }

    public NoSortList<decimal> Caliber { get; }

    public NoSortList<int> BarrelCount { get; }

    public NoSortList<string> BarrelsLayout { get; }

    public decimal? Range { get; }

    public List<Dispersion> DispersionData { get; }

    public List<double> DispersionModifier { get; }

    public NoSortList<decimal> Reload { get; }

    public NoSortList<decimal> RoF { get; }

    public NoSortList<string> Dpm { get; }

    public NoSortList<decimal> Fpm { get; }

    public decimal? Sigma { get; }

    // Secondary shells
    public string? Type { get; }

    public NoSortList<decimal> Mass { get; }

    public NoSortList<decimal> Damage { get; }

    public NoSortList<decimal> SplashRadius { get; }

    public NoSortList<decimal> SplashDamage { get; }

    public NoSortList<int> Penetration { get; }

    public NoSortList<decimal> Speed { get; }

    public NoSortList<decimal> AirDrag { get; }

    public NoSortList<decimal> HeShellFireChance { get; }

    public NoSortList<decimal> HeBlastRadius { get; }

    public NoSortList<decimal> HeBlastPenetration { get; }

    public NoSortList<decimal> SapOvermatch { get; }

    public NoSortList<string> SapRicochet { get; }
}
