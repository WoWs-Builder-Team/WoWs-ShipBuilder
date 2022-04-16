using DnetIndexedDb;
using Microsoft.JSInterop;

namespace WoWsShipBuilder.Web.Data;

public class GameDataDb : IndexedDbInterop
{
    public GameDataDb(IJSRuntime jsRuntime, IndexedDbOptions<GameDataDb> indexedDbDatabaseOptions)
        : base(jsRuntime, indexedDbDatabaseOptions)
    {
    }
}
