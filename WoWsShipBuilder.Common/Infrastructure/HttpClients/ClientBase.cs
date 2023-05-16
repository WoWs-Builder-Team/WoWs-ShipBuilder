using Newtonsoft.Json;
using WoWsShipBuilder.Infrastructure.Data;

// ReSharper disable VirtualMemberNeverOverridden.Global
namespace WoWsShipBuilder.Infrastructure.HttpClients;

public abstract class ClientBase
{
    protected ClientBase(IDataService dataService, IAppDataService appDataService)
    {
        DataService = dataService;
        AppDataService = appDataService;
    }

    protected IAppDataService AppDataService { get; }

    protected IDataService DataService { get; }

    protected abstract HttpClient Client { get; }

    protected virtual async Task DownloadFileAsync(Uri uri, string fileName)
    {
        await using Stream stream = await Client.GetStreamAsync(uri);
        await DataService.StoreAsync(stream, fileName);
    }

    protected virtual async Task<T?> GetJsonAsync<T>(string url, JsonSerializer? customSerializer = null)
    {
        await using Stream stream = await Client.GetStreamAsync(url);
        return GetJson<T>(stream, customSerializer);
    }

    internal T? GetJson<T>(Stream stream, JsonSerializer? customSerializer = null)
    {
        using var streamReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(streamReader);
        JsonSerializer serializer = customSerializer ?? new JsonSerializer();
        return serializer.Deserialize<T>(jsonReader);
    }
}
