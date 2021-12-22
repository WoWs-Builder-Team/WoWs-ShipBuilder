using System;
using Avalonia.Controls;
using Avalonia.Metadata;
using Newtonsoft.Json;
using ReactiveUI;
using WoWsShipBuilder.Core;
using WoWsShipBuilder.Core.BuildCreator;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.UI.UserControls;
using WoWsShipBuilder.UI.Utilities;

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

        private string? buildString;

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

        public async void LoadFromImage(object parameter)
        {
            var fileDialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Directory = AppDataHelper.Instance.BuildImageOutputDirectory,
                Filters = new()
                {
                    new()
                    {
                        Name = "PNG Files",
                        Extensions = new() { "png" },
                    },
                },
            };

            string[]? result = await fileDialog.ShowAsync(self);
            if (result is not { Length: > 0 })
            {
                return;
            }

            string buildJson = BuildImageProcessor.ExtractBuildData(result[0]);
            var build = JsonConvert.DeserializeObject<Build>(buildJson);

            if (build == null)
            {
                await MessageBox.Show(self, "Could not read build data from file. Has the file been compressed?", "Invalid file", MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                return;
            }

            ProcessLoadedBuild(build);
        }

        public async void Import(object parameter)
        {
            Build build;
            Logging.Logger.Info("Trying to import build string: {0}", BuildString);
            try
            {
                build = Build.CreateBuildFromString(BuildString!);
                Logging.Logger.Info("Build correctly created");
            }
            catch (Exception e)
            {
                Logging.Logger.Warn(e, "Error in creating the build.");
                await MessageBox.Show(self, "The Build string is not in the correct format.", "Invalid string.", MessageBox.MessageBoxButtons.Ok, MessageBox.MessageBoxIcon.Error);
                return;
            }

            ProcessLoadedBuild(build);
        }

        [DependsOn(nameof(BuildString))]
        public bool CanImport(object parameter)
        {
            return !string.IsNullOrWhiteSpace(BuildString);
        }

        private void ProcessLoadedBuild(Build build)
        {
            if (!ImportOnly)
            {
                Logging.Logger.Info("Adding build to saved ones.");
                AppData.Builds.Insert(0, build);
            }

            self.Close(build);
        }
    }
}
