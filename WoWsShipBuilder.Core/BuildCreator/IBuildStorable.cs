using System.Collections.Generic;

namespace WoWsShipBuilder.Core.BuildCreator
{
    public interface IBuildStorable
    {
        void LoadBuild(IEnumerable<string> storedData);

        List<string> SaveBuild();
    }
}
