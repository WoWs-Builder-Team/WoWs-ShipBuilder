using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.Infrastructure;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public class SkillItemViewModel : ViewModelBase
{
    private readonly Dictionary<int, bool> canAddCache;

    private readonly Dictionary<int, bool> canRemoveCache;

    private readonly CaptainSkillSelectorViewModel parent;

    private readonly ShipClass shipClass;

    private bool canExecute;

    // TODO: update to new nullability state
    public SkillItemViewModel(Skill skill, CaptainSkillSelectorViewModel parent, ShipClass shipClass, Dictionary<int, bool> canAddCache, Dictionary<int, bool> canRemoveCache)
    {
        Skill = skill;
        SkillTier = skill.Tiers.First(x => x.ShipClass == shipClass).Tier;
        SkillXPosition = skill.Tiers.First(x => x.ShipClass == shipClass).XPosition;
        this.canAddCache = canAddCache;
        this.canRemoveCache = canRemoveCache;
        if (skill.Modifiers is not null)
        {
            Modifiers = skill.Modifiers.Where(x => !x.Key.Contains('_') || x.Key.Contains("_" + shipClass)).ToDictionary(x => x.Key, x => x.Value);
        }

        if (skill.ConditionalModifiers is not null)
        {
            ConditionalModifiers = skill.ConditionalModifiers.Where(x => !x.Key.Contains('_') || x.Key.Contains("_" + shipClass)).ToDictionary(x => x.Key, x => x.Value);
        }

        this.shipClass = shipClass;
        this.parent = parent;
        CanExecuteChanged();
    }

    public Skill Skill { get; }

    public int SkillTier { get; }

    public int SkillXPosition { get; }

    public Dictionary<string, float> Modifiers { get; } = new();

    public Dictionary<string, float> ConditionalModifiers { get; } = new();

    public bool CanExecute
    {
        get => canExecute;
        set => this.RaiseAndSetIfChanged(ref canExecute, value);
    }

    public void CanExecuteChanged()
    {
        bool result;

        // Can add skill
        if (!parent.SkillOrderList.Contains(Skill))
        {
            if (canAddCache.TryGetValue(SkillTier, out bool canAdd))
            {
                CanExecute = canAdd;
                return;
            }

            // If the points would go over 21, can't add the skill
            if (parent.AssignedPoints + SkillTier + 1 > 21)
            {
                result = false;
            }

            // If it's a skill of the first tier, i can always add it
            else if (SkillTier == 0)
            {
                result = true;
            }
            else
            {
                // If it's not, i search the skill of the previous tier. If at least one exist, i can add it
                result = parent.SkillOrderList.Select(s => s.Tiers.First(x => x.ShipClass == shipClass).Tier).Any(cost => cost == SkillTier - 1);
            }

            canAddCache[SkillTier] = result;
        }

        // Can remove skill
        else
        {
            // If the skill tier is 4 (tier 3, since our index start from 0), can always remove
            if (SkillTier == 3)
            {
                result = true;
            }
            else
            {
                if (canRemoveCache.TryGetValue(SkillTier, out bool canRemove))
                {
                    CanExecute = canRemove;
                    return;
                }

                List<int> skillTiers = parent.SkillOrderList.Select(iteratedSkill => iteratedSkill.Tiers.First(x => x.ShipClass == shipClass).Tier).ToList();
                int nextTierSkillCount = skillTiers.Count(skillCost => skillCost > SkillTier);
                int sameTierSkillCount = skillTiers.Count(skillCost => skillCost == SkillTier);

                if (nextTierSkillCount > 0)
                {
                    result = sameTierSkillCount > 1;
                }
                else
                {
                    result = true;
                }

                canRemoveCache[SkillTier] = result;
            }
        }

        CanExecute = result;
    }
}
