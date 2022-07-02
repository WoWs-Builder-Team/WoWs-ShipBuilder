namespace WoWsShipBuilder.Web.WebWorkers;

public static class WebWorkerServiceCollectionHelper
{
    public delegate void Configure(IServiceCollection services);

    public static IServiceProvider BuildServiceProviderFromMethod(Configure configureMethod)
    {
        var serviceCollection = new ServiceCollection();
        configureMethod(serviceCollection);
        return serviceCollection.BuildServiceProvider();
    }
}
