using WoWsShipBuilder.DataStructures.Modifiers;
using RxUnit = System.Reactive.Unit;

namespace WoWsShipBuilder.Features.ShipStats;

/// <summary>
/// A selectable part of a build, like the module, upgrade, consumable, captain skill or signal selection.
/// <br/>
/// Implementing this interface allows a viewmodel to take part in the aggregation done by the viewmodels owning it,
/// so that adding a new build component does not require updating every place that listens for changes,
/// collects modifiers or checks whether the user customized the build.
/// <br/>
/// This interface covers the live state of a component. Storing that state in a build is a separate concern,
/// covered by <see cref="IBuildComponentProvider"/>, because not every component stores its selection as a list of strings.
/// </summary>
public interface IBuildComponent
{
    /// <summary>
    /// Gets an observable that emits whenever a change to this component requires the ship stats to be recalculated.
    /// </summary>
    IObservable<RxUnit> Changes { get; }

    /// <summary>
    /// Gets a value indicating whether the current selection differs from the default one.
    /// </summary>
    bool IsModified { get; }

    /// <summary>
    /// Gets the modifiers contributed by the current selection.
    /// </summary>
    /// <returns>The modifiers of this component, empty if it does not contribute any.</returns>
    IEnumerable<Modifier> GetModifiers();
}
