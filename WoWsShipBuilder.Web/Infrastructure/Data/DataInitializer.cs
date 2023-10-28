using Microsoft.Extensions.Options;
using WoWsShipBuilder.Infrastructure;
using WoWsShipBuilder.Infrastructure.ApplicationData;
using WoWsShipBuilder.Infrastructure.Localization;
using WoWsShipBuilder.Infrastructure.Utility;

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
        await this.localizationProvider.RefreshDataAsync(this.cdnOptions.Server, AppConstants.SupportedLanguages.ToArray());
        if (this.appDataService is ServerAppDataService serverAppDataService)
        {
            if (this.cdnOptions.UseLocalFiles)
            {
                await serverAppDataService.LoadLocalFilesAsync(this.cdnOptions.Server);
            }
            else
            {
                await serverAppDataService.FetchData();
            }
        }
    }
}
