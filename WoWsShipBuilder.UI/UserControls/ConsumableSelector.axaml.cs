using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using WoWsShipBuilder.Core.DataUI;
using WoWsShipBuilder.UI.Converters;

namespace WoWsShipBuilder.UI.UserControls
{
    public class ConsumableSelector : UserControl
    {
        #region Static Fields and Constants

        private static readonly ImagePathConverter Converter = new();

        public static readonly StyledProperty<List<ConsumableUI>> ConsumableListProperty =
            AvaloniaProperty.Register<ConsumableSelector, List<ConsumableUI>>(nameof(ConsumableList), notifying: ConsumableListChanged);

        public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<ConsumableSelector, int>(nameof(SelectedIndex), notifying: OnSelectedIndexChanged);

        public static readonly StyledProperty<Action<ConsumableUI>?> SelectedIndexChangedProperty =
            AvaloniaProperty.Register<ConsumableSelector, Action<ConsumableUI>?>(nameof(SelectedIndexChanged));

        private static readonly StyledProperty<IImage> SelectedImageProperty = AvaloniaProperty.Register<UpgradeSelector, IImage>(
            nameof(SelectedImage));

        #endregion

        public ConsumableSelector()
        {
            InitializeComponent();
        }

        private Popup ConsumablePopup => this.FindControl<Popup>("Popup");

        public int SelectedIndex
        {
            get => GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public List<ConsumableUI> ConsumableList
        {
            get => GetValue(ConsumableListProperty);
            set => SetValue(ConsumableListProperty, value);
        }

        public Action<ConsumableUI>? SelectedIndexChanged
        {
            get => GetValue(SelectedIndexChangedProperty);
            set => SetValue(SelectedIndexChangedProperty, value);
        }

        private IImage SelectedImage
        {
            get => GetValue(SelectedImageProperty);
            set => SetValue(SelectedImageProperty, value);
        }

        private static void OnSelectedIndexChanged(IAvaloniaObject sender, bool beforeNotify)
        {
            if (!beforeNotify)
            {
                var selector = (ConsumableSelector)sender;
                var consumable = selector.ConsumableList[selector.SelectedIndex];
                selector.SelectedImage = (IImage)Converter.Convert(consumable, typeof(IImage), null!, CultureInfo.InvariantCulture);
                selector.ConsumablePopup.IsOpen = false;
                selector.SelectedIndexChanged?.Invoke(consumable);
            }
        }

        private static void ConsumableListChanged(IAvaloniaObject sender, bool beforeNotify)
        {
            if (!beforeNotify)
            {
                var selector = (ConsumableSelector)sender;
                selector.SelectedImage = (IImage)Converter.Convert(selector.ConsumableList[selector.SelectedIndex], typeof(IImage), null!, CultureInfo.InvariantCulture);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void DropDownButton_OnClick(object? sender, RoutedEventArgs e)
        {
            if (ConsumableList.Count > 1)
            {
                ConsumablePopup.IsOpen = true;
            }
        }
    }
}
