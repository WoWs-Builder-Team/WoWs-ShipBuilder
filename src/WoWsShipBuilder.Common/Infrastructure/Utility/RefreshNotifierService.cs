namespace WoWsShipBuilder.Infrastructure.Utility;

public class RefreshNotifierService
{
    public event Action? RefreshRequested;

    public void NotifyRefreshRequested() => this.RefreshRequested?.Invoke();
}
