using System.Collections.Immutable;
using ReactiveUI;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public partial class SkillActivationItemViewModel : ReactiveObject
{
    [Observable]
    private int activationNumbers;

    [Observable]
    private string skillName = default!;

    [Observable]
    private bool status;

    public SkillActivationItemViewModel(string name, int skillId, ImmutableList<Modifier> modifiers, bool activationStatus, int maximumActivations = 0, int activationNumbers = 1, string description = "")
    {
        this.SkillName = name;
        this.Status = activationStatus;
        this.SkillId = skillId;
        this.MaximumActivations = maximumActivations == -1 ? 99 : maximumActivations;
        this.ActivationNumbers = activationNumbers;
        this.Modifiers = modifiers;
        this.Description = description;
    }

    public ImmutableList<Modifier> Modifiers { get; }

    public int SkillId { get; }

    public int MaximumActivations { get; }

    public string Description { get; }

    /// <summary>
    /// Gets a description key to try if <see cref="Description"/> does not resolve. Talent descriptions are keyed per
    /// battle group and the game ships no variant for every group, so the regular one acts as the fallback.
    /// </summary>
    public string DescriptionFallback { get; init; } = string.Empty;

    /// <summary>
    /// Gets the escalation steps of a tiered talent, ordered by level. Empty for a talent that does not escalate.
    /// </summary>
    public ImmutableList<TalentTierViewModel> Tiers { get; init; } = ImmutableList<TalentTierViewModel>.Empty;

    public bool IsTiered => !this.Tiers.IsEmpty;

    /// <summary>
    /// Gets the tier currently reached, based on how many times the talent has been activated. Clamped to the ladder
    /// so that it is never null for a tiered talent: falling through to <see cref="Modifiers"/> would report the
    /// fully escalated values, which are stronger than any tier the player has actually reached.
    /// </summary>
    public TalentTierViewModel? ActiveTier => this.Tiers.IsEmpty ? null : this.Tiers[Math.Clamp(this.ActivationNumbers, 1, this.Tiers.Count) - 1];

    /// <summary>
    /// Gets the stats actually in effect: the active tier's for a tiered talent, the flat list otherwise.
    /// </summary>
    public ImmutableList<Modifier> EffectiveModifiers => this.ActiveTier?.Modifiers ?? this.Modifiers;
}
