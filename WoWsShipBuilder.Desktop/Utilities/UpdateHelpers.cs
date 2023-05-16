using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.Desktop.Settings;
using WoWsShipBuilder.Desktop.UserControls;

namespace WoWsShipBuilder.Desktop.Utilities;

public static class UpdateHelpers
{
    public static async Task<MessageBox.MessageBoxResult> ShowUpdateRestartDialog(Window? parent)
    {
        return await Dispatcher.UIThread.InvokeAsync(async () => await MessageBox.Show(
            parent,
            AppSettingsHelper.LocalizerInstance.GetAppLocalization(nameof(Translation.UpdateMessageBox_Description)).Localization,
            AppSettingsHelper.LocalizerInstance.GetAppLocalization(nameof(Translation.UpdateMessageBox_Title)).Localization,
            MessageBox.MessageBoxButtons.YesNo,
            MessageBox.MessageBoxIcon.Question));
    }
}
