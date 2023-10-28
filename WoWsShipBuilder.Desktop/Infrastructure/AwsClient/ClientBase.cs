using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.Desktop.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.ApplicationData;

// ReSharper disable VirtualMemberNeverOverridden.Global
namespace WoWsShipBuilder.Desktop.Infrastructure.AwsClient;

public abstract class ClientBase
{
    protected ClientBase(IDataService dataService, IAppDataService appDataService)
    {
        this.DataService = dataService;
        this.AppDataService = appDataService;
    }

    protected IAppDataService AppDataService { get; }

    protected IDataService DataService { get; }

    protected abstract HttpClient Client { get; }

    protected virtual async Task DownloadFileAsync(Uri uri, string fileName)
    {
        await using Stream stream = await this.Client.GetStreamAsync(uri);
        await this.DataService.StoreAsync(stream, fileName);
    }

    protected virtual async Task<T?> GetJsonAsync<T>(string url, JsonSerializer? customSerializer = null)
    {
        await using Stream stream = await this.Client.GetStreamAsync(url);
        return this.GetJson<T>(stream, customSerializer);
    }

    internal T? GetJson<T>(Stream stream, JsonSerializer? customSerializer = null)
    {
        using var streamReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(streamReader);
        JsonSerializer serializer = customSerializer ?? new JsonSerializer();
        return serializer.Deserialize<T>(jsonReader);
    }
}
