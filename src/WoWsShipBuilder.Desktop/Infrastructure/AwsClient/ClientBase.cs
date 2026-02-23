using System;
using System.Net.Http;
using System.Threading.Tasks;
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
        await using var stream = await this.Client.GetStreamAsync(uri);
        await this.DataService.StoreAsync(stream, fileName);
    }
}
