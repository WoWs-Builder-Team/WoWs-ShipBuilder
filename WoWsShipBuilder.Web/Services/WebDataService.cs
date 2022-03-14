namespace WoWsShipBuilder.Web.Services;

using System.Diagnostics.CodeAnalysis;
using BlazorDB;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Web.Data;

public class WebDataService : IDataService
{
    private readonly IBlazorDbFactory dbFactory;

    private IndexedDbManager? dbManager;

    public WebDataService(IBlazorDbFactory dbFactory)
    {
        this.dbFactory = dbFactory;
    }

    public async Task StoreStringAsync(string content, string path)
    {
        await UpdateDbManager();
        var addResult = await dbManager.AddRecordAsync(new StoreRecord<GameDataRecord>
        {
            StoreName = AppData.Settings.SelectedServerType.StringName(),
            Record = new(path, content),
        });
        if (addResult.Failed)
        {
            await dbManager.UpdateRecord(new UpdateRecord<GameDataRecord>
            {
                StoreName = AppData.Settings.SelectedServerType.StringName(),
                Record = new(path, content),
                Key = path,
            });
        }
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

    public async Task<string> LoadStringAsync(string path)
    {
        await UpdateDbManager();
        var dataRecord = await dbManager.GetRecordByIdAsync<string, GameDataRecord>(AppData.Settings.SelectedServerType.StringName(), path);
        return dataRecord.Content;
    }

    public async Task<T?> LoadAsync<T>(string path)
    {
        string stringContent = await LoadStringAsync(path);
        return JsonConvert.DeserializeObject<T>(stringContent);
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

    [MemberNotNull(nameof(dbManager))]
    private async Task UpdateDbManager()
    {
        // dbManager is not null after this method but apparently, Roslyn does not want to recognize that.
#pragma warning disable CS8774
        dbManager ??= await dbFactory.GetDbManager("data");
#pragma warning restore CS8774
    }
}
