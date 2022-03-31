using BlazorWorker.Extensions.JSRuntime;
using BlazorWorker.WorkerCore;
using DnetIndexedDb;
using DnetIndexedDb.Fluent;
using DnetIndexedDb.Models;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Web.Data;

namespace WoWsShipBuilder.Web.Services;

public class StartupWebWorkerService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IWorkerMessageService workerMessageService;

    public StartupWebWorkerService(IWorkerMessageService workerMessageService)
    {
        this.workerMessageService = workerMessageService;
        serviceProvider = ServiceCollectionHelper.BuildServiceProviderFromMethod(Configure);
    }

    public T Resolve<T>() => serviceProvider.GetService<T>();

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

        services.AddTransient<IAppDataService, WebAppDataService>()
            .AddTransient<IDataService, WebDataService>()
            .AddTransient<WebWorkerTestService>()
            .AddSingleton(workerMessageService)
            .AddBlazorWorkerJsRuntime();
    }
}
