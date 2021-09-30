using System.Collections.Generic;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.Core.DataProvider
{
    public interface ILocalDataProvider
    {
        public string AppDataDirectory { get; }

        public string GetDataPath(ServerType serverType);

        public Dictionary<string, T>? ReadLocalJsonData<T>(Nation? nation, ServerType serverType);
    }
}
