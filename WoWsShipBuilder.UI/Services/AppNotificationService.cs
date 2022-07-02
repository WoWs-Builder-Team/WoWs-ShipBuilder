using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using WoWsShipBuilder.Core.Localization;
using WoWsShipBuilder.ViewModels.Base;

namespace WoWsShipBuilder.UI.Services;

/// <summary>
/// A service that contains information about application updates and their state.
/// Can also be used for other application information that should be available globally in the app.
/// This service is an implementation of <see cref="ViewModelBase"/> and allows views to bind to its properties, supporting property-changed events.
/// </summary>
public class AppNotificationService : ViewModelBase
{
    private string appUpdateMessage = nameof(Translation.Placeholder);

    private UpdateStatus appUpdateStatus = UpdateStatus.None;

    /// <summary>
    /// Gets or sets the current <see cref="UpdateStatus"/> of the application.
    /// </summary>
    public UpdateStatus AppUpdateStatus
    {
        get => appUpdateStatus;
        set => this.RaiseAndSetIfChanged(ref appUpdateStatus, value);
    }

    /// <summary>
    /// Gets or sets an optional message about the current update status of the application.
    /// <br />
    /// <b>This property is meant to contain a localization key so the frontend can localize the message.</b>
    /// </summary>
    public string AppUpdateMessage
    {
        get => appUpdateMessage;
        set => this.RaiseAndSetIfChanged(ref appUpdateMessage, value);
    }

    /// <summary>
    /// Sets the <see cref="AppUpdateStatus"/> property to <see cref="UpdateStatus.Active"/> and ensures that the operation is executed on the UI thread, eliminating concurrency issues.
    /// </summary>
    public async Task NotifyAppUpdateStart()
    {
        await Dispatcher.UIThread.InvokeAsync(() => AppUpdateStatus = UpdateStatus.Active);
    }

    /// <summary>
    /// Sets the <see cref="AppUpdateStatus"/> property to <see cref="UpdateStatus.Completed"/> and ensures that the operation is executed on the UI thread, eliminating concurrency issues.
    /// </summary>
    public async Task NotifyAppUpdateComplete()
    {
        await Dispatcher.UIThread.InvokeAsync(() => AppUpdateStatus = UpdateStatus.Completed);
    }

    /// <summary>
    /// Sets the <see cref="AppUpdateStatus"/> property to <see cref="UpdateStatus.Error"/> and ensures that the operation is executed on the UI thread, eliminating concurrency issues.
    /// Allows to set an optional message for <see cref="AppUpdateMessage"/>.
    /// </summary>
    /// <param name="message">An optional message that is set as <see cref="AppUpdateMessage"/>.</param>
    public async Task NotifyAppUpdateError(string? message = null)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            AppUpdateStatus = UpdateStatus.Error;
            if (!string.IsNullOrWhiteSpace(message))
            {
                AppUpdateMessage = message;
            }
        });
    }
}
