using Microsoft.JSInterop;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.Web.Data;

namespace WoWsShipBuilder.Web.WebWorkers;

public class WebWorkerDataService : IDataService
{
    private readonly GameDataDb gameDataDb;
    private readonly IJSRuntime jsRuntime;

    private bool initialized;

    public WebWorkerDataService(GameDataDb gameDataDb, IJSRuntime jsRuntime)
    {
        this.gameDataDb = gameDataDb;
        this.jsRuntime = jsRuntime;
    }

    private string CurrentStoreName => AppData.Settings.SelectedServerType.StringName();

    public async Task StoreStringAsync(string content, string path)
    {
        path = path.Replace(".json", string.Empty).Replace("..", ".");
        await OpenDb();
        await gameDataDb.UpdateItems<GameDataDto>(CurrentStoreName, new() { new(path, content) });
    }

    public async Task StoreAsync(object content, string path)
    {
        string contentString = JsonConvert.SerializeObject(content);
        await StoreStringAsync(contentString, path);
    }

    public async Task StoreAsync(Stream stream, string path)
    {
        using var reader = new StreamReader(stream);
        string stringContent = await reader.ReadToEndAsync();
        await StoreStringAsync(stringContent, path);
    }

    public void Store(object content, string path)
    {
        throw new NotSupportedException();
    }

    public void Store(Stream stream, string path)
    {
       throw new NotSupportedException();
    }

    public async Task<string?> LoadStringAsync(string path)
    {
        await OpenDb();
        var dataRecord = await gameDataDb.GetByKey<string, GameDataDto>(CurrentStoreName, path);
        return dataRecord?.Content;
    }

    public async Task<T?> LoadAsync<T>(string path)
    {
        string? stringContent = await LoadStringAsync(path);
        return stringContent is not null ? JsonConvert.DeserializeObject<T>(stringContent) : default;
    }

    public T? Load<T>(string path)
    {
        throw new NotSupportedException();
    }

    public string CombinePaths(params string[] paths)
    {
        IEnumerable<string> cleanPaths = paths.Select(path => path.Replace('/', '.').Trim('.'));
        return string.Join('.', cleanPaths);
    }

    public async Task LoadAllShipsAsync(ServerType serverType)
    {
        await OpenDb();
        var keyBase = $"{serverType.StringName()}.Ship";
        List<GameDataDto>? dataList = await gameDataDb.GetRange<string, GameDataDto>(serverType.StringName(), keyBase, keyBase + '\uffff');
        var combinedData = new List<KeyValuePair<string, Ship>>();
        foreach (var dto in dataList)
        {
            combinedData.AddRange(JsonConvert.DeserializeObject<Dictionary<string, Ship>>(dto.Content) ?? new Dictionary<string, Ship>());
            await Task.Yield();
        }

        AppData.ShipDictionary = combinedData.ToDictionary(x => x.Key, x => x.Value);
    }

    private async Task OpenDb()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        await this.jsRuntime.InvokeVoidAsync("eval", "(() => { self.window = self; return null; })()");
        await this.jsRuntime.InvokeVoidAsync("importLocalScripts", "_content/DnetIndexedDb/rxjs.min.js");
        await this.jsRuntime.InvokeVoidAsync("importLocalScripts", "_content/DnetIndexedDb/dnet-indexeddb.js");
        await gameDataDb.OpenIndexedDb();
    }
}
