namespace WoWsShipBuilder.Web.Services;

public static class ServiceCollectionHelper
{
    public delegate void Configure(IServiceCollection services);

    public static IServiceProvider BuildServiceProviderFromMethod(Configure configureMethod)
    {
        var serviceCollection = new ServiceCollection();
        configureMethod(serviceCollection);
        return serviceCollection.BuildServiceProvider();
    }
}
