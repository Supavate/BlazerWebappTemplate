namespace MyWebApp.Navigation;

public sealed class NavigationRefreshService
{
    public event Action? NavigationChanged;

    public void NotifyChanged() => NavigationChanged?.Invoke();
}