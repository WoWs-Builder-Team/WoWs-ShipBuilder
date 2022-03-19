namespace WoWsShipBuilder.Web.Services;

using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Web.Data;

public class WebDataService : IDataService
{
    private readonly GameDataDb gameDataDb;

    private bool initialized;

    public WebDataService(GameDataDb gameDataDb)
    {
        this.gameDataDb = gameDataDb;
    }

    private string CurrentStoreName => AppData.Settings.SelectedServerType.StringName();

    public async Task StoreStringAsync(string content, string path)
    {
        path = path.Replace(".json", string.Empty);
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

    private async Task OpenDb()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        await gameDataDb.OpenIndexedDb();
    }
}
