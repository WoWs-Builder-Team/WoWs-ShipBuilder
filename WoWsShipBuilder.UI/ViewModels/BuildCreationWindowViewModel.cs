using System.Windows.Input;
using Avalonia;
using ReactiveUI;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.UI.Views;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class BuildCreationWindowViewModel : ViewModelBase
    {
        private BuildCreationWindow self;
        private Build build;

        public BuildCreationWindowViewModel(BuildCreationWindow win, Build currentBuild)
        {
            self = win;
            build = currentBuild;
            SaveBuildCommand = ReactiveCommand.Create(() => SaveBuild());
            CloseBuildCommand = ReactiveCommand.Create(() => CloseBuild());
        }

        private string? buildName;

        public string? BuildName
        {
            get => buildName;
            set => this.RaiseAndSetIfChanged(ref buildName, value);
        }

        public ICommand SaveBuildCommand { get; }

        public ICommand CloseBuildCommand { get; }

        private void SaveBuild()
        {
            var buildString = BuildCreator.CreateStringFromBuild(build);
            Application.Current.Clipboard.SetTextAsync(buildString);
        }

        private void CloseBuild()
        {
            self.Close();
        }
    }
}
