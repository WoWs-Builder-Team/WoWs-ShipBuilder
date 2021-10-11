using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class ConsumableSelector : UserControl
    {
        #region Static Fields and Constants

        private static readonly ConsumableImageConverter Converter = new();

        public static readonly StyledProperty<List<(ShipConsumable, Consumable)>> ConsumableListProperty =
            AvaloniaProperty.Register<ConsumableSelector, List<(ShipConsumable, Consumable)>>(nameof(ConsumableList), notifying: ConsumableListChanged);

        public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<ConsumableSelector, int>(nameof(SelectedIndex), notifying: OnSelectedIndexChanged);

        public static readonly StyledProperty<Action<ShipConsumable>?> SelectedIndexChangedProperty =
            AvaloniaProperty.Register<ConsumableSelector, Action<ShipConsumable>?>(nameof(SelectedIndexChanged));

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

        public List<(ShipConsumable, Consumable)> ConsumableList
        {
            get => GetValue(ConsumableListProperty);
            set => SetValue(ConsumableListProperty, value);
        }

        public Action<ShipConsumable>? SelectedIndexChanged
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
                var (shipConsumable, consumable) = selector.ConsumableList[selector.SelectedIndex];
                selector.SelectedImage = (IImage)Converter.Convert(consumable, typeof(IImage), null!, CultureInfo.InvariantCulture);
                selector.ConsumablePopup.IsOpen = false;
                selector.SelectedIndexChanged?.Invoke(shipConsumable);
            }
        }

        private static void ConsumableListChanged(IAvaloniaObject sender, bool beforeNotify)
        {
            if (!beforeNotify)
            {
                var selector = (ConsumableSelector)sender;
                selector.SelectedImage = (IImage)Converter.Convert(selector.ConsumableList[selector.SelectedIndex].Item2, typeof(IImage), null!, CultureInfo.InvariantCulture);
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
