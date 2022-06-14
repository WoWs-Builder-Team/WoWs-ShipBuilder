using System.Collections.Generic;

namespace WoWsShipBuilder.Core.BuildCreator;

public interface IBuildStorable
{
    /// <summary>
    /// Loads data from a stored build into the current instance.
    /// </summary>
    /// <param name="storedData">An <see cref="IEnumerable{T}"/> containing stored build data.</param>
    void LoadBuild(IEnumerable<string> storedData);

    /// <summary>
    /// Saves data from the current instance for a build.
    /// </summary>
    /// <returns>A list containing the new build data.</returns>
    List<string> SaveBuild();
}
