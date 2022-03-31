using Microsoft.JSInterop;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;

namespace WoWsShipBuilder.Web.Services;

public class WebWorkerTestService
{
    private IDataService dataService;
    private IJSRuntime jsRuntime;

    private static Dictionary<string, Ship>? cache;

    public WebWorkerTestService(IDataService dataService, IJSRuntime jsRuntime)
    {
        this.dataService = dataService;
        this.jsRuntime = jsRuntime;
    }

    // public async Task<ArtilleryShell> GetShell(string shellIndex)
    // {
    //     await this.jsRuntime.InvokeVoidAsync("eval", "(() => { self.window = self; return null; })()");
    //     await this.jsRuntime.InvokeVoidAsync("importLocalScripts", "_content/DnetIndexedDb/rxjs.min.js");
    //     await this.jsRuntime.InvokeVoidAsync("importLocalScripts", "_content/DnetIndexedDb/dnet-indexeddb.js");
    //
    //     return await appDataService.GetProjectile<ArtilleryShell>(shellIndex);
    // }

    public async Task<Ship> GetShip()
    {
        await this.jsRuntime.InvokeVoidAsync("eval", "(() => { self.window = self; return null; })()");
        await this.jsRuntime.InvokeVoidAsync("importLocalScripts", "_content/DnetIndexedDb/rxjs.min.js");
        await this.jsRuntime.InvokeVoidAsync("importLocalScripts", "_content/DnetIndexedDb/dnet-indexeddb.js");

        string dataLocation = dataService.CombinePaths(GetDataPath(ServerType.Live), "Ship", "Russia");
        Console.WriteLine("Cache count = " + cache?.Count);

        cache ??= await DeserializeFromDb<Dictionary<string, Ship>>(dataLocation);
        var ship = cache!.First().Value;
        return ship;

    }

    public string GetDataPath(ServerType serverType)
    {
        return serverType.StringName();
    }

    internal async Task<T?> DeserializeFromDb<T>(string dataLocation)
    {
        if (string.IsNullOrWhiteSpace(dataLocation))
        {
            throw new ArgumentException("The provided data location must not be empty.");
        }

        var returnValue = await dataService.LoadAsync<T>(dataLocation);

        return returnValue;
    }
}
