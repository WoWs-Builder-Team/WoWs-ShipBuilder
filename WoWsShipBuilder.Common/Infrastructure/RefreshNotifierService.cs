namespace WoWsShipBuilder.Common.Infrastructure;

public class RefreshNotifierService
{
    public event Action? RefreshRequested;

    public void NotifyRefreshRequested() => RefreshRequested?.Invoke();
}
