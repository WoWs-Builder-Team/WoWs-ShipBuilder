using MudBlazor;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Infrastructure.ApplicationTheme;

public class ThemeManager
{
    private const int BorderRadius = 4;

    private readonly AppSettings appSettings;

    private ThemeEnums.ThemeStyle currentThemeStyle;

    private string currentPrimaryColor;

    public const string DefaultPrimaryColor = "#6186FF";

    public const ThemeEnums.ThemeVariant DefaultThemeVariant = ThemeEnums.ThemeVariant.Dark;

    public const ThemeEnums.ThemeStyle DefaultThemeStyle = ThemeEnums.ThemeStyle.Rounded;

    public ThemeManager(AppSettings appSettings)
    {
        this.appSettings = appSettings;
        this.currentPrimaryColor = this.appSettings.ThemePrimaryColor ?? DefaultPrimaryColor;
        this.currentThemeStyle = this.appSettings.ThemeStyle ?? DefaultThemeStyle;
        this.AppTheme = this.GetAppTheme();
    }

    public event EventHandler<MudTheme>? ThemeChanged;

    public MudTheme AppTheme { get; private set; }

    private MudTheme GetAppTheme()
    {
        return Helpers.IsAprilFool() ? this.CreateAprilFoolTheme() : this.CreateTheme();
    }

    private MudTheme CreateAprilFoolTheme()
    {
        return new()
        {
            LayoutProperties =
            {
                DefaultBorderRadius = $"{Random.Shared.Next(0, BorderRadius * 50) / 10}px",
            },
            PaletteDark =
            {
                Black = $"{Helpers.GenerateRandomColor()}FF",
                White = $"{Helpers.GenerateRandomColor()}FF",
                Primary = $"{Helpers.GenerateRandomColor()}FF",
                PrimaryContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Secondary = $"{Helpers.GenerateRandomColor()}FF",
                Info = $"{Helpers.GenerateRandomColor()}FF",
                InfoContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Success = $"{Helpers.GenerateRandomColor()}FF",
                SuccessContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Warning = $"{Helpers.GenerateRandomColor()}FF",
                WarningContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Error = $"{Helpers.GenerateRandomColor()}FF",
                ErrorContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Dark = $"{Helpers.GenerateRandomColor()}FF",
                DarkContrastText = $"{Helpers.GenerateRandomColor()}FF",
                TextPrimary = $"{Helpers.GenerateRandomColor()}FF",
                TextSecondary = $"{Helpers.GenerateRandomColor()}89",
                TextDisabled = $"{Helpers.GenerateRandomColor()}60",
                ActionDefault = $"{Helpers.GenerateRandomColor()}89",
                ActionDisabled = $"{Helpers.GenerateRandomColor()}42",
                ActionDisabledBackground = $"{Helpers.GenerateRandomColor()}1E",
                Surface = $"{Helpers.GenerateRandomColor()}FF",
                LinesDefault = $"{Helpers.GenerateRandomColor()}4D",
                LinesInputs = $"{Helpers.GenerateRandomColor()}",
                AppbarBackground = $"{Helpers.GenerateRandomColor()}FF",
                HoverOpacity = Random.Shared.NextDouble(),
                DrawerBackground = $"{Helpers.GenerateRandomColor()}",
                DividerLight = $"{Helpers.GenerateRandomColor()}CC",
                BackgroundGray = $"{Helpers.GenerateRandomColor()}FF",
                SecondaryContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Tertiary = $"{Helpers.GenerateRandomColor()}FF",
                TertiaryContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Background = $"{Helpers.GenerateRandomColor()}FF",
                GrayDark = $"{Helpers.GenerateRandomColor()}FF",
            },
            PaletteLight =
            {
                Black = $"{Helpers.GenerateRandomColor()}FF",
                White = $"{Helpers.GenerateRandomColor()}FF",
                Primary = $"{Helpers.GenerateRandomColor()}FF",
                PrimaryContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Secondary = $"{Helpers.GenerateRandomColor()}FF",
                Info = $"{Helpers.GenerateRandomColor()}FF",
                InfoContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Success = $"{Helpers.GenerateRandomColor()}FF",
                SuccessContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Warning = $"{Helpers.GenerateRandomColor()}FF",
                WarningContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Error = $"{Helpers.GenerateRandomColor()}FF",
                ErrorContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Dark = $"{Helpers.GenerateRandomColor()}FF",
                DarkContrastText = $"{Helpers.GenerateRandomColor()}FF",
                TextPrimary = $"{Helpers.GenerateRandomColor()}FF",
                TextSecondary = $"{Helpers.GenerateRandomColor()}89",
                TextDisabled = $"{Helpers.GenerateRandomColor()}60",
                ActionDefault = $"{Helpers.GenerateRandomColor()}89",
                ActionDisabled = $"{Helpers.GenerateRandomColor()}42",
                ActionDisabledBackground = $"{Helpers.GenerateRandomColor()}1E",
                Surface = $"{Helpers.GenerateRandomColor()}FF",
                LinesDefault = $"{Helpers.GenerateRandomColor()}4D",
                LinesInputs = $"{Helpers.GenerateRandomColor()}",
                AppbarBackground = $"{Helpers.GenerateRandomColor()}FF",
                HoverOpacity = Random.Shared.NextDouble(),
                DrawerBackground = $"{Helpers.GenerateRandomColor()}",
                DividerLight = $"{Helpers.GenerateRandomColor()}CC",
                BackgroundGray = $"{Helpers.GenerateRandomColor()}FF",
                SecondaryContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Tertiary = $"{Helpers.GenerateRandomColor()}FF",
                TertiaryContrastText = $"{Helpers.GenerateRandomColor()}FF",
                Background = $"{Helpers.GenerateRandomColor()}FF",
                GrayDark = $"{Helpers.GenerateRandomColor()}FF",
            },
            ZIndex =
            {
                Dialog = 2000,
                Popover = 3000,
                Tooltip = 4000,
            },
        };
    }

    private MudTheme CreateTheme()
    {
        return new()
        {
            LayoutProperties =
            {
                DefaultBorderRadius = this.currentThemeStyle == ThemeEnums.ThemeStyle.Sharp ? "0" : $"{BorderRadius}px",
            },
            PaletteLight =
            {
                Primary = this.currentPrimaryColor,
                TextPrimary = "#000000FF",
                PrimaryContrastText = "#FFFFFF",
                Secondary = "#000000FF",
                SecondaryContrastText = "#FFFFFF",
                Tertiary = "#F0B105",
                TertiaryContrastText = "#FFFFFF",
                AppbarBackground = this.currentPrimaryColor,
                AppbarText = "#000000FF",
                Background = "#FFFFFF",
                DrawerBackground = "#EDEDED",
                Surface = "#EDEDED",
                GrayDark = this.currentPrimaryColor,
                HoverOpacity = 0.165,
                Black = "#000000FF",
                GrayDarker = "#000000FF",
            },
            PaletteDark =
            {
                Black = "#121212FF",
                White = "#FDFDFDFF",
                Primary = this.currentPrimaryColor,
                PrimaryContrastText = "#242424",
                Secondary = "#D4D4D4",
                SecondaryContrastText = "#282828",
                Tertiary = "#F0B105",
                TertiaryContrastText = "#282828",
                InfoContrastText = "#FDFDFDFF",
                Success = "#00CD42",
                SuccessContrastText = "#282828",
                WarningContrastText = "#282828",
                Error = "#BF0000FF",
                ErrorContrastText = "#FDFDFDFF",
                Dark = "#505050",
                DarkContrastText = "#FDFDFDFF",
                DrawerBackground = "#232323",
                Surface = "#232323",
                HoverOpacity = 0.165,
                AppbarBackground = "#121212FF",
                Background = "#282828",
                BackgroundGray = "#1E1E1E",
                GrayDark = "#505050",
            },
            ZIndex =
            {
                Dialog = 2000,
                Popover = 3000,
                Tooltip = 4000,
            },
        };
    }

    public void UpdateTheme()
    {
        object? sender = null;
        if (this.currentPrimaryColor != this.appSettings.ThemePrimaryColor || this.currentThemeStyle != this.appSettings.ThemeStyle)
        {
            this.currentPrimaryColor = this.appSettings.ThemePrimaryColor ?? DefaultPrimaryColor;
            this.currentThemeStyle = this.appSettings.ThemeStyle ?? DefaultThemeStyle;
            this.AppTheme = this.GetAppTheme();
            sender = this;
        }

        this.ThemeChanged?.Invoke(sender, this.AppTheme);
    }
}
