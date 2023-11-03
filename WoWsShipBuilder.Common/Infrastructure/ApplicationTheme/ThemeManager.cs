using MudBlazor;
using WoWsShipBuilder.Features.Settings;
using WoWsShipBuilder.Infrastructure.Utility;

namespace WoWsShipBuilder.Infrastructure.ApplicationTheme;

public class ThemeManager
{
    private const int BorderRadius = 4;

    private readonly AppSettings appSettings;

    public ThemeManager(AppSettings appSettings)
    {
        this.appSettings = appSettings;
        this.AppTheme = this.GetAppTheme();
    }

    public event EventHandler<MudTheme>? ThemeChanged;

    public enum ThemeVariant
    {
        Dark,
        White,
        AprilFool,
    }

    public enum ThemeStyle
    {
        Rounded,
        Sharp,
    }

    public string DefaultPrimaryColor => "#6186FF";

    public ThemeVariant DefaultThemeVariant => ThemeVariant.Dark;

    public ThemeStyle DefaultThemeStyle => ThemeStyle.Rounded;

    public MudTheme AppTheme { get; private set; }

    private MudTheme GetAppTheme()
    {
        ThemeVariant? selectedThemeVariant = Helpers.IsAprilFool() ? ThemeVariant.AprilFool : this.appSettings.ThemeVariant ?? this.DefaultThemeVariant;
        return selectedThemeVariant switch
        {
            ThemeVariant.Dark => this.CreateDarkTheme(),
            ThemeVariant.White => this.CreateLightTheme(),
            ThemeVariant.AprilFool => this.CreateAprilFoolTheme(),
            _ => this.CreateDarkTheme(),
        };
    }

    private MudTheme CreateDarkTheme()
    {
        return new()
        {
            LayoutProperties =
            {
                DefaultBorderRadius = (this.appSettings.ThemeStyle ?? this.DefaultThemeStyle) == ThemeStyle.Sharp ? "0" : $"{BorderRadius}px",
            },
            PaletteDark =
            {
                Black = "#121212FF",
                White = "#FDFDFDFF",
                Primary = this.appSettings.ThemePrimaryColor ?? this.DefaultPrimaryColor,
                PrimaryContrastText = "#242424",
                Secondary = "#D4D4D4",
                SecondaryContrastText = "#282828",
                Tertiary = "#FFD700",
                TertiaryContrastText = "#282828",
                InfoContrastText = "#FDFDFDFF",
                Success = "#00CD42",
                SuccessContrastText = "#282828",
                WarningContrastText = "#282828",
                Error = "#BF0000FF",
                ErrorContrastText = "#FDFDFDFF",
                Dark = "#505050",
                DarkContrastText = "#FDFDFDFF",
                Surface = "#232323",
                HoverOpacity = 0.165,
                AppbarBackground = "#121212FF",
                Background = "#282828",
                BackgroundGrey = "#1E1E1E",
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

    private MudTheme CreateAprilFoolTheme()
    {
        return new()
        {
            LayoutProperties =
            {
                DefaultBorderRadius = $"{Random.Shared.Next(0, BorderRadius * 20) / 10}px",
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
                HoverOpacity = new Random().NextDouble(),
                DrawerBackground = $"{Helpers.GenerateRandomColor()}",
                DividerLight = $"{Helpers.GenerateRandomColor()}CC",
                BackgroundGrey = $"{Helpers.GenerateRandomColor()}FF",
            },
            ZIndex =
            {
                Dialog = 2000,
                Popover = 3000,
                Tooltip = 4000,
            },
        };
    }

    private MudTheme CreateLightTheme()
    {
        throw new NotImplementedException();
    }

    public void UpdateTheme()
    {
        this.AppTheme = this.GetAppTheme();
        this.ThemeChanged?.Invoke(this, this.AppTheme);
    }
}
