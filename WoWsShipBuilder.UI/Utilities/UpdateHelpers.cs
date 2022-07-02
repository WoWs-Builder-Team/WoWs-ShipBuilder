using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.UI.Settings;
using WoWsShipBuilder.UI.UserControls;

namespace WoWsShipBuilder.UI.Utilities;

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
