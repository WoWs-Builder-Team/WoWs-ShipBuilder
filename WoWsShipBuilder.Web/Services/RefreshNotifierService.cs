namespace WoWsShipBuilder.Web.Services;

public class RefreshNotifierService
{
    public event Action? RefreshRequested;

    public void NotifyRefreshRequested() => RefreshRequested?.Invoke();
}
