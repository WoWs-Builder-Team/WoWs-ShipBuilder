using BlazorWorker.Extensions.JSRuntime;
using BlazorWorker.WorkerCore;
using DnetIndexedDb;
using DnetIndexedDb.Fluent;
using DnetIndexedDb.Models;
using WoWsShipBuilder.Web.Data;

namespace WoWsShipBuilder.Web.WebWorkers;

public class StartupWebWorkerService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IWorkerMessageService workerMessageService;

    public StartupWebWorkerService(IWorkerMessageService workerMessageService)
    {
        this.workerMessageService = workerMessageService;
        serviceProvider = WebWorkerServiceCollectionHelper.BuildServiceProviderFromMethod(Configure);
    }

    public T Resolve<T>() where T : notnull => serviceProvider.GetRequiredService<T>();

    public void Configure(IServiceCollection services)
    {
        var gameDataDbModel = new IndexedDbDatabaseModel().WithName("data").WithVersion(1);
        gameDataDbModel.AddStore<GameDataDto>("live");
        gameDataDbModel.AddStore<GameDataDto>("pts");
#if DEBUG
        gameDataDbModel.AddStore<GameDataDto>("dev1");
        gameDataDbModel.AddStore<GameDataDto>("dev2");
        gameDataDbModel.AddStore<GameDataDto>("dev3");
#endif
        services.AddIndexedDbDatabase<GameDataDb>(options => { options.UseDatabase(gameDataDbModel); });

        services.AddSingleton<WebWorkerDataService>()
            .AddSingleton(workerMessageService)
            .AddBlazorWorkerJsRuntime();
    }
}
