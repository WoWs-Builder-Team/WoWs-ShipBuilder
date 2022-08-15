using Microsoft.Extensions.Options;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Web.Data;

namespace WoWsShipBuilder.Web.Services;

public class DataInitializer
{
    private readonly IOptions<CdnOptions> cdnOptions;

    private readonly ILocalizationProvider localizationProvider;

    private readonly IAppDataService appDataService;

    public DataInitializer(IOptions<CdnOptions> cdnOptions, ILocalizationProvider localizationProvider, IAppDataService appDataService)
    {
        this.cdnOptions = cdnOptions;
        this.localizationProvider = localizationProvider;
        this.appDataService = appDataService;
    }

    public async Task InitializeData()
    {
        await localizationProvider.RefreshDataAsync(cdnOptions.Value.Server, AppConstants.SupportedLanguages.ToArray());
        if (appDataService is ServerAppDataService serverAppDataService)
        {
            await serverAppDataService.FetchData();
            AppData.ShipSummaryList = await serverAppDataService.GetShipSummaryList(cdnOptions.Value.Server);
        }
    }
}
