using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Metadata;
using ReactiveUI;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.UserControls;

namespace WoWsShipBuilder.UI.ViewModels
{
    public class BuildImportViewModel : ViewModelBase
    {
        private Window self;

        public BuildImportViewModel(Window win)
        {
            self = win;
        }

        private bool importOnly = true;

        public bool ImportOnly
        {
            get => importOnly;
            set => this.RaiseAndSetIfChanged(ref importOnly, value);
        }

        private string? buildString = default!;

        public string? BuildString
        {
            get => buildString;
            set => this.RaiseAndSetIfChanged(ref buildString, value);
        }

        public void Cancel()
        {
            BuildString = null;
            self.Close();
        }

        public async void Import(object parameter)
        {
            Build build;
            try
            {
                build = Build.CreateBuildFromString(BuildString!);
            }
            catch (Exception)
            {
                await MessageBox.Show(self, "The Build string is not in the correct format.", "Invalid string.", MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                return;
            }

            if (!ImportOnly)
            {
                AppData.Builds.Insert(0, build);
            }

            self.Close(build);
        }

        [DependsOn(nameof(BuildString))]
        public bool CanImport(object parameter)
        {
            return !string.IsNullOrWhiteSpace(BuildString);
        }
    }
}
