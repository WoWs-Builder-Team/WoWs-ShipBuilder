using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Metadata;
using ReactiveUI;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilderDataStructures;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class ShipSelectionWindowViewModel : ViewModelBase
    {
        private readonly Window? self;

        private CancellationTokenSource tokenSource;

        public ShipSelectionWindowViewModel()
            : this(null)
        {
        }

        public ShipSelectionWindowViewModel(Window? win)
        {
            self = win;
            tokenSource = new CancellationTokenSource();

            AppData.ShipSummaryList ??= AppDataHelper.Instance.GetShipSummaryList(AppData.Settings.SelectedServerType);

            Dictionary<string, ShipSummary> shipNameDictionary = AppData.ShipSummaryList.ToDictionary(ship => Localizer.Instance[$"{ship.Index}_FULL"].Localization, ship => ship);
            FilteredShipNameDictionary = new SortedDictionary<string, ShipSummary>(shipNameDictionary);
            SummaryList = new AvaloniaList<KeyValuePair<string, ShipSummary>>(FilteredShipNameDictionary.Select(entry => entry));
        }

        public AvaloniaList<KeyValuePair<string, ShipSummary>> SummaryList { get; }

        private bool tierFilterChecked;

        public bool TierFilterChecked
        {
            get => tierFilterChecked;
            set
            {
                this.RaiseAndSetIfChanged(ref tierFilterChecked, value);
                if (!value)
                {
                    SelectedTier = "";
                    ApplyFilter();
                }
            }
        }

        private bool classFilterChecked;

        public bool ClassFilterChecked
        {
            get => classFilterChecked;
            set
            {
                this.RaiseAndSetIfChanged(ref classFilterChecked, value);
                if (!value)
                {
                    SelectedClass = null;
                    ApplyFilter();
                }
            }
        }

        private bool nationFilterChecked;

        public bool NationFilterChecked
        {
            get => nationFilterChecked;
            set
            {
                this.RaiseAndSetIfChanged(ref nationFilterChecked, value);
                if (!value)
                {
                    SelectedNation = null;
                    ApplyFilter();
                }
            }
        }

        private bool typeFilterChecked;

        public bool TypeFilterChecked
        {
            get => typeFilterChecked;
            set
            {
                this.RaiseAndSetIfChanged(ref typeFilterChecked, value);
                if (!value)
                {
                    SelectedType = null;
                    ApplyFilter();
                }
            }
        }

        private List<string> tierList = new() { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X" };

        public List<string> TierList
        {
            get => tierList;
            set => this.RaiseAndSetIfChanged(ref tierList, value);
        }

        public List<ShipClass> ClassList { get; } = Enum.GetValues<ShipClass>().Except(new List<ShipClass> { ShipClass.Auxiliary }).ToList();

        public List<Nation> NationList { get; } = Enum.GetValues<Nation>().Except(new List<Nation> { Nation.Common }).ToList();

        public List<ShipCategory> TypeList { get; } =
            Enum.GetValues<ShipCategory>().Except(new List<ShipCategory> { ShipCategory.Disabled, ShipCategory.Clan }).ToList();

        private string? selectedTier;

        public string? SelectedTier
        {
            get => selectedTier;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedTier, value);
                ApplyFilter();
            }
        }

        private ShipClass? selectedClass;

        public ShipClass? SelectedClass
        {
            get => selectedClass;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedClass, value);
                ApplyFilter();
            }
        }

        private Nation? selectedNation;

        public Nation? SelectedNation
        {
            get => selectedNation;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedNation, value);
                ApplyFilter();
            }
        }

        private ShipCategory? selectedType;

        public ShipCategory? SelectedType
        {
            get => selectedType;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedType, value);
                ApplyFilter();
            }
        }

        private SortedDictionary<string, ShipSummary> FilteredShipNameDictionary { get; }

        private KeyValuePair<string, ShipSummary>? selectedShip;

        public KeyValuePair<string, ShipSummary>? SelectedShip
        {
            get => selectedShip;
            set => this.RaiseAndSetIfChanged(ref selectedShip, value);
        }

        private string inputText = string.Empty;

        public string InputText
        {
            get => inputText;
            set
            {
                this.RaiseAndSetIfChanged(ref inputText, value);
                tokenSource.Cancel();
                tokenSource.Dispose();
                tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                Task.Run(
                    async () =>
                    {
                        await Task.Delay(150);
                        ApplyFilter(token);
                    },
                    token);
            }
        }

        private readonly SemaphoreSlim semaphore = new(1, 1);

        private void ApplyFilter()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            ApplyFilter(token);
        }

        private void ApplyFilter(CancellationToken token)
        {
            string searchText = InputText;
            Task.Run(
                async () =>
                {
                    var items = FilteredShipNameDictionary.Where(pair => SummaryFilter(pair, searchText)).ToList();
                    await semaphore.WaitAsync(token);
                    if (!token.IsCancellationRequested)
                    {
                        SummaryList.Clear();
                        SummaryList.AddRange(items);
                    }

                    semaphore.Release();
                },
                token);
        }

        private bool SummaryFilter(KeyValuePair<string, ShipSummary> valuePair, string textSearch)
        {
            var shipSummary = valuePair.Value;
            bool result = !(TierFilterChecked && shipSummary.Tier != TierList.IndexOf(SelectedTier!) + 1);

            if (result && ClassFilterChecked && shipSummary.ShipClass != SelectedClass)
            {
                result = false;
            }

            if (result && NationFilterChecked && SelectedNation != null && shipSummary.Nation != SelectedNation)
            {
                result = false;
            }

            if (result && TypeFilterChecked && SelectedType != null && shipSummary.Category != SelectedType)
            {
                return false;
            }

            if (result && !string.IsNullOrWhiteSpace(textSearch))
            {
                result = CultureInfo.CurrentCulture.CompareInfo.IndexOf(valuePair.Key, textSearch, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) !=
                         -1;
            }

            return result;
        }

        public void Confirm(object parameter)
        {
            self?.Close(SelectedShip?.Value);
        }

        [DependsOn(nameof(SelectedShip))]
        public bool CanConfirm(object parameter)
        {
            return SelectedShip?.Value != null;
        }
    }
}
