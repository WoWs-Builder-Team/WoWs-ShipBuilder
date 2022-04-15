using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;
using WoWsShipBuilder.Core.Settings;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.ViewModels.Helper
{
    public class ShipSelectionWindowViewModel : ViewModelBase
    {
        private CancellationTokenSource tokenSource;

        public ShipSelectionWindowViewModel()
            : this(false, new())
        {
        }

        public ShipSelectionWindowViewModel(bool multiSelection, Dictionary<string, ShipSummary> availableShips)
        {
            tokenSource = new();

            FilteredShipNameDictionary = new(availableShips);
            SummaryList = new(FilteredShipNameDictionary.Select(entry => entry));
            MultiSelectionEnabled = multiSelection;

            var canConfirmExecute = this.WhenAnyValue(x => x.SelectedShipList.Count, count => count > 0);
            ConfirmCommand = ReactiveCommand.CreateFromTask(Confirm, canConfirmExecute);
        }

        public static async Task<Dictionary<string, ShipSummary>> LoadParamsAsync(IAppDataService appDataService, AppSettings appSettings)
        {
            AppData.ShipSummaryList ??= await appDataService.GetShipSummaryList(appSettings.SelectedServerType);
            return AppData.ShipSummaryList.ToDictionary(ship => Localizer.Instance[$"{ship.Index}_FULL"].Localization, ship => ship);
        }

        public bool MultiSelectionEnabled { get; }

        public ICommand ConfirmCommand { get; }

        public CustomObservableCollection<KeyValuePair<string, ShipSummary>> SummaryList { get; }

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

        private List<string> tierList = new() { "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI" };

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

        public SortedDictionary<string, ShipSummary> FilteredShipNameDictionary { get; }

        public CustomObservableCollection<KeyValuePair<string, ShipSummary>> SelectedShipList { get; } = new();

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

        public Interaction<List<ShipSummary>?, Unit> CloseInteraction { get; } = new();

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

        public bool SummaryFilter(KeyValuePair<string, ShipSummary> valuePair, string textSearch)
        {
            var shipSummary = valuePair.Value;
            bool result = !(TierFilterChecked && !string.IsNullOrWhiteSpace(SelectedTier) && shipSummary.Tier != TierList.IndexOf(SelectedTier!) + 1);

            if (result && ClassFilterChecked && SelectedClass != null && shipSummary.ShipClass != SelectedClass)
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
                result = CultureInfo.CurrentCulture.CompareInfo.IndexOf(valuePair.Key, textSearch, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != -1;
            }

            return result;
        }

        private async Task Confirm()
        {
            List<ShipSummary> result = MultiSelectionEnabled ? SelectedShipList.Select(x => x.Value).ToList() : new() { SelectedShipList.First().Value };
            await CloseInteraction.Handle(result);
        }
    }
}
