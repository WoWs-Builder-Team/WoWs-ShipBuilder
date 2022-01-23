using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI.Views
{
    public partial class DispersionShipRemovalDialog : Window
    {
        public DispersionShipRemovalDialog()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static Task<List<string>> ShowShipRemoval(Window parent, List<string> shipList)
        {
            var win = new DispersionShipRemovalDialog
            {
                DataContext = new DispersionShipRemovalViewModel(shipList),
                ShowInTaskbar = false,
            };

            var button = win.FindControl<Button>("Ok");

            button.Click += (_, __) =>
            {
                win.Close();
            };

            var tcs = new TaskCompletionSource<List<string>>();
            win.Closed += (sender, e) =>
            {
                var model = win.DataContext as DispersionShipRemovalViewModel;
                tcs.TrySetResult(model!.ShipsToDeleteList.ToList());
            };

            win.ShowDialog(parent);

            return tcs.Task;
        }
    }
}
