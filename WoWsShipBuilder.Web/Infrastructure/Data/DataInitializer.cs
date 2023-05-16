using Microsoft.Extensions.Options;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.Data;
using WoWsShipBuilder.Infrastructure.Localization;

namespace WoWsShipBuilder.Web.Infrastructure.Data;

public class DataInitializer
{
    private readonly CdnOptions cdnOptions;

    private readonly ILocalizationProvider localizationProvider;

    private readonly IAppDataService appDataService;

    public DataInitializer(IOptions<CdnOptions> cdnOptions, ILocalizationProvider localizationProvider, IAppDataService appDataService)
    {
        this.cdnOptions = cdnOptions.Value;
        this.localizationProvider = localizationProvider;
        this.appDataService = appDataService;
    }

    public async Task InitializeData()
    {
        await localizationProvider.RefreshDataAsync(cdnOptions.Server, AppConstants.SupportedLanguages.ToArray());
        if (appDataService is ServerAppDataService serverAppDataService)
        {
            if (cdnOptions.UseLocalFiles)
            {
                await serverAppDataService.LoadLocalFilesAsync(cdnOptions.Server);
            }
            else
            {
                await serverAppDataService.FetchData();
            }
        }
    }
}
