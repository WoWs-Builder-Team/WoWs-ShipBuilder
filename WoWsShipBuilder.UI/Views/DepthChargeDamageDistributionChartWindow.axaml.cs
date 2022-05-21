using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace WoWsShipBuilder.UI.Views;

public class DepthChargeDamageDistributionChartWindow : Window
{
    public DepthChargeDamageDistributionChartWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
