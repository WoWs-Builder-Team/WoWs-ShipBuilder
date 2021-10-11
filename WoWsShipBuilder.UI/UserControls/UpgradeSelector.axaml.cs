using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public class UpgradeSelector : UserControl
    {
        #region Static Fields and Constants

        private static readonly ImagePathConverter Converter = new();

        public static readonly StyledProperty<int> SelectedIndexProperty =
            AvaloniaProperty.Register<UpgradeSelector, int>(nameof(SelectedIndex), 0, notifying: SelectedIndexChanged);

        public static readonly StyledProperty<List<Modernization>> AvailableModernizationsProperty =
            AvaloniaProperty.Register<UpgradeSelector, List<Modernization>>(nameof(AvailableModernizations), notifying: ModernizationListChanged);

        public static readonly StyledProperty<Action<Modernization?, List<Modernization>>?> SelectedModernizationChangedProperty =
            AvaloniaProperty.Register<UpgradeSelector, Action<Modernization?, List<Modernization>>?>(nameof(SelectedModernizationChanged));

        private static readonly StyledProperty<IImage> SelectedImageProperty = AvaloniaProperty.Register<UpgradeSelector, IImage>(
            nameof(SelectedImage),
            (IImage)Converter.Convert(DataHelper.PlaceholderModernization, typeof(IImage), "Modernization", CultureInfo.InvariantCulture));

        private static readonly StyledProperty<IReadOnlyList<Modernization>> EffectiveModernizationsListProperty =
            AvaloniaProperty.Register<UpgradeSelector, IReadOnlyList<Modernization>>(nameof(EffectiveModernizationsList));

        #endregion

        public UpgradeSelector()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the list of available upgrades for this control. This list should not contain the placeholder upgrade object.
        /// </summary>
        public List<Modernization> AvailableModernizations
        {
            get => GetValue(AvailableModernizationsProperty);
            set => SetValue(AvailableModernizationsProperty, value);
        }

        /// <summary>
        /// Gets or sets a callback for a change of the currently selected upgrade.
        /// </summary>
        public Action<Modernization?, List<Modernization>>? SelectedModernizationChanged
        {
            get => GetValue(SelectedModernizationChangedProperty);
            set => SetValue(SelectedModernizationChangedProperty, value);
        }

        /// <summary>
        /// Gets or sets the currently selected index. Index 0 will always be the placeholder modification, indicating that the user did not select an upgrade for this slot.
        /// </summary>
        public int SelectedIndex
        {
            get => GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        /// <summary>
        /// Gets or sets the image shown as background of the button that opens the control's popup.
        /// </summary>
        private IImage SelectedImage
        {
            get => GetValue(SelectedImageProperty);
            set => SetValue(SelectedImageProperty, value);
        }

        /// <summary>
        /// Gets or sets the effective upgrade list, a concatenation of the placeholder modernization object and the <see cref="AvailableModernizations"/> property.
        /// </summary>
        private IReadOnlyList<Modernization> EffectiveModernizationsList
        {
            get => GetValue(EffectiveModernizationsListProperty);
            set => SetValue(EffectiveModernizationsListProperty, value);
        }

        /// <summary>
        /// Gets the popup that shows the list of available upgrades.
        /// </summary>
        private Popup UpgradePopup => this.FindControl<Popup>("UpgradeListPopup");

        /// <summary>
        /// Handles a change of the <see cref="SelectedIndexProperty"/> of an instance of this control.
        /// </summary>
        /// <param name="sender">The control that was changed.</param>
        /// <param name="beforeNotify">A bool indicating whether this method was called before the property changed notify.</param>
        private static void SelectedIndexChanged(IAvaloniaObject sender, bool beforeNotify)
        {
            if (!beforeNotify)
            {
                var dropDown = (UpgradeSelector)sender;
                Modernization? newSelection = dropDown.SelectedIndex > 0 ? dropDown.EffectiveModernizationsList[dropDown.SelectedIndex] : null;
                dropDown.SelectedImage = (IImage)Converter.Convert(newSelection, typeof(IImage), null!, CultureInfo.InvariantCulture);
                dropDown.UpgradePopup.IsOpen = false;
                dropDown.SelectedModernizationChanged?.Invoke(newSelection, dropDown.AvailableModernizations);
            }
        }

        /// <summary>
        /// Handles a change of the list of the <see cref="AvailableModernizationsProperty"/> of an instance of this control.
        /// </summary>
        /// <param name="sender">The control that was changed.</param>
        /// <param name="beforeNotify">A bool indicating whether this method was called before the property changed notify.</param>
        private static void ModernizationListChanged(IAvaloniaObject sender, bool beforeNotify)
        {
            if (!beforeNotify)
            {
                var dropDown = (UpgradeSelector)sender;
                dropDown.EffectiveModernizationsList = DataHelper.PlaceholderBaseList
                    .Concat(dropDown.AvailableModernizations)
                    .ToList();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // ReSharper disable twice UnusedParameter.Local
        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            UpgradePopup.IsOpen = true;
        }
    }
}
