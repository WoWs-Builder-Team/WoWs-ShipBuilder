using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using WoWsShipBuilder.UI.Converters;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.UserControls
{
    /// <summary>
    /// A UserControl that allows to open a drop-down list of available upgrades. Clicking on one selects the upgrade.
    /// Similar functionality to a <see cref="ComboBox"/>. However, unlike the combobox, the images within the list do not get disposed unexpectedly.
    /// </summary>
    public class DropDownSelector : UserControl
    {
        private static readonly ModernizationImageConverter Converter = new();

        public static readonly StyledProperty<IImage> SelectedImageProperty = AvaloniaProperty.Register<DropDownSelector, IImage>(
            nameof(SelectedImage),
            (IImage)Converter.Convert(DataHelper.PlaceholderModernization, typeof(IImage), null!, CultureInfo.InvariantCulture));

        public static readonly StyledProperty<int> SelectedIndexProperty =
            AvaloniaProperty.Register<DropDownSelector, int>(nameof(SelectedIndex), 0, notifying: SelectedIndexChanged);

        public static readonly StyledProperty<List<Modernization>> AvailableModernizationsProperty =
            AvaloniaProperty.Register<DropDownSelector, List<Modernization>>(nameof(AvailableModernizations));

        public static readonly StyledProperty<ICommand> UpgradeSelectionCommandProperty =
            AvaloniaProperty.Register<DropDownSelector, ICommand>(nameof(UpgradeSelectionCommand));

        public DropDownSelector()
        {
            InitializeComponent();
        }

        public IImage SelectedImage
        {
            get => GetValue(SelectedImageProperty);
            private set => SetValue(SelectedImageProperty, value);
        }

        public int SelectedIndex
        {
            get => GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public List<Modernization> AvailableModernizations
        {
            get => GetValue(AvailableModernizationsProperty);
            set => SetValue(AvailableModernizationsProperty, value);
        }

        public ICommand UpgradeSelectionCommand
        {
            get => GetValue(UpgradeSelectionCommandProperty);
            set => SetValue(UpgradeSelectionCommandProperty, value);
        }

        private Popup UpgradePopup => this.FindControl<Popup>("UpgradeListPopup");

        private static void SelectedIndexChanged(IAvaloniaObject sender, bool beforeNotify)
        {
            if (!beforeNotify)
            {
                var dropDown = (DropDownSelector)sender;
                dropDown.SelectedImage = (IImage)Converter.Convert(dropDown.AvailableModernizations[dropDown.SelectedIndex], typeof(IImage), null!, CultureInfo.InvariantCulture);
                dropDown.UpgradePopup.IsOpen = false;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // ReSharper disable once UnusedParameter.Local
        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            UpgradePopup.IsOpen = true;
        }
    }
}
