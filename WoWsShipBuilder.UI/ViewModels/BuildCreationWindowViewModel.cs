using System.Windows.Input;
using Avalonia;
using ReactiveUI;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.Translations;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class BuildCreationWindowViewModel : ViewModelBase
    {
        private BuildCreationWindow self;
        private Build build;

        public BuildCreationWindowViewModel(BuildCreationWindow win, Build currentBuild, string shipName)
        {
            self = win;
            build = currentBuild;
            SaveBuildCommand = ReactiveCommand.Create(() => SaveBuild());
            CloseBuildCommand = ReactiveCommand.Create(() => CloseBuild());
            ShipName = shipName;
        }

        private string shipName = default!;

        public string ShipName
        {
            get => shipName;
            set => this.RaiseAndSetIfChanged(ref shipName, value);
        }

        private string? buildName;

        public string? BuildName
        {
            get => buildName;
            set => this.RaiseAndSetIfChanged(ref buildName, value);
        }

        public ICommand SaveBuildCommand { get; }

        public ICommand CloseBuildCommand { get; }

        private async void SaveBuild()
        {
            build.BuildName = BuildName + " - " + ShipName;
            var buildString = build.CreateStringFromBuild();
            AppData.Builds.Insert(0, build);
            await Application.Current!.Clipboard!.SetTextAsync(buildString);
            await MessageBox.Show(self, Translation.BuildCreationWindow_SavedClipboard, Translation.BuildCreationWindow_BuildSaved, MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Info);
            self.Close();
        }

        private void CloseBuild()
        {
            self.Close();
        }
    }
}
