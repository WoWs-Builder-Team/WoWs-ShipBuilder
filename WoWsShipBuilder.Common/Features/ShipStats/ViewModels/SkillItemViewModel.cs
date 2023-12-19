using ReactiveUI;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace WoWsShipBuilder.Features.ShipStats.ViewModels;

public class SkillItemViewModel : ReactiveObject
{
    private readonly Dictionary<int, bool> canAddCache;

    private readonly Dictionary<int, bool> canRemoveCache;

    private readonly CaptainSkillSelectorViewModel parent;

    private readonly ShipClass shipClass;

    private bool canExecute;

    public SkillItemViewModel(Skill skill, CaptainSkillSelectorViewModel parent, ShipClass shipClass, Dictionary<int, bool> canAddCache, Dictionary<int, bool> canRemoveCache)
    {
        this.Skill = skill;
        this.SkillPosition = skill.Tiers.First(x => x.ShipClass == shipClass);
        this.SkillTier = this.SkillPosition.Tier;
        this.canAddCache = canAddCache;
        this.canRemoveCache = canRemoveCache;
        this.Modifiers = skill.Modifiers.Where(x => !x.Name.Contains('_') || x.Name.StartsWith("repeatable_", StringComparison.Ordinal) || x.Name.Contains("_" + shipClass)).ToList();

        // TODO: fix modifiers not being filtered per class. do the same of the normal modifiers basically.
        this.ConditionalModifierGroups = skill.ConditionalModifierGroups.ToList();

        this.shipClass = shipClass;
        this.parent = parent;
        this.CanExecuteChanged();
    }

    public Skill Skill { get; }

    public int SkillTier { get; }

    public SkillPosition SkillPosition { get; }

    public List<Modifier> Modifiers { get; }

    public List<ConditionalModifierGroup> ConditionalModifierGroups { get; }

    public bool CanExecute
    {
        get => this.canExecute;
        set => this.RaiseAndSetIfChanged(ref this.canExecute, value);
    }

    public void CanExecuteChanged()
    {
        bool result = !this.parent.SkillOrderList.Contains(this.Skill) ? this.CanAddSkill() : this.CanRemoveSkill();
        this.CanExecute = result;
    }

    private bool CanAddSkill()
    {
        bool result;
        if (this.canAddCache.TryGetValue(this.SkillTier, out bool canAdd))
        {
            return canAdd;
        }

        // If the points would go over 21, can't add the skill
        if (this.parent.AssignedPoints + this.SkillTier + 1 > 21)
        {
            result = false;
        }

        // If it's a skill of the first tier, i can always add it
        else if (this.SkillTier == 0)
        {
            result = true;
        }
        else
        {
            // If it's not, i search the skill of the previous tier. If at least one exist, i can add it
            result = this.parent.SkillOrderList.Select(s => s.Tiers.First(x => x.ShipClass == this.shipClass).Tier).Any(cost => cost == this.SkillTier - 1);
        }

        this.canAddCache[this.SkillTier] = result;
        return result;
    }

    private bool CanRemoveSkill()
    {
        // If the skill tier is 4 (tier 3, since our index start from 0), can always remove
        if (this.SkillTier == 3)
        {
            return true;
        }

        if (this.canRemoveCache.TryGetValue(this.SkillTier, out bool canRemove))
        {
            return canRemove;
        }

        var skillTiers = this.parent.SkillOrderList.Select(iteratedSkill => iteratedSkill.Tiers.First(x => x.ShipClass == this.shipClass).Tier).ToList();
        int nextTierSkillCount = skillTiers.Count(skillCost => skillCost > this.SkillTier);
        int sameTierSkillCount = skillTiers.Count(skillCost => skillCost == this.SkillTier);

        bool result;
        if (nextTierSkillCount > 0)
        {
            result = sameTierSkillCount > 1;
        }
        else
        {
            result = true;
        }

        this.canRemoveCache[this.SkillTier] = result;
        return result;
    }
}
