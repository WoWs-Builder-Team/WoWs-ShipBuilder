﻿using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;
using WoWsShipBuilder.Infrastructure.Localization.Resources;

namespace WoWsShipBuilder.Desktop.Infrastructure;

/// <summary>
/// A service that contains information about application updates and their state.
/// Can also be used for other application information that should be available globally in the app.
/// This service is an implementation of <see cref="ReactiveObject"/> and allows views to bind to its properties, supporting property-changed events.
/// </summary>
public class AppNotificationService : ReactiveObject
{
    private string appUpdateMessage = nameof(Translation.Placeholder);

    private UpdateStatus appUpdateStatus = UpdateStatus.None;

    /// <summary>
    /// Gets or sets the current <see cref="UpdateStatus"/> of the application.
    /// </summary>
    public UpdateStatus AppUpdateStatus
    {
        get => this.appUpdateStatus;
        set => this.RaiseAndSetIfChanged(ref this.appUpdateStatus, value);
    }

    /// <summary>
    /// Gets or sets an optional message about the current update status of the application.
    /// <br />
    /// <b>This property is meant to contain a localization key so the frontend can localize the message.</b>
    /// </summary>
    public string AppUpdateMessage
    {
        get => this.appUpdateMessage;
        set => this.RaiseAndSetIfChanged(ref this.appUpdateMessage, value);
    }

    /// <summary>
    /// Sets the <see cref="AppUpdateStatus"/> property to <see cref="UpdateStatus.Active"/> and ensures that the operation is executed on the UI thread, eliminating concurrency issues.
    /// </summary>
    public async Task NotifyAppUpdateStart()
    {
        await Dispatcher.UIThread.InvokeAsync(() => this.AppUpdateStatus = UpdateStatus.Active);
    }

    /// <summary>
    /// Sets the <see cref="AppUpdateStatus"/> property to <see cref="UpdateStatus.Completed"/> and ensures that the operation is executed on the UI thread, eliminating concurrency issues.
    /// </summary>
    public async Task NotifyAppUpdateComplete()
    {
        await Dispatcher.UIThread.InvokeAsync(() => this.AppUpdateStatus = UpdateStatus.Completed);
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
            this.AppUpdateStatus = UpdateStatus.Error;
            if (!string.IsNullOrWhiteSpace(message))
            {
                this.AppUpdateMessage = message;
            }
        });
    }
}
