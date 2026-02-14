using rUI.Avalonia.Desktop.Controls;

namespace rUI.Avalonia.Desktop.Services;

public interface IInfoBarService
{
    void RegisterHost(InfoBarControl infoBar);
    Task ShowAsync(Action<InfoBarControl>? configure = null);
    Task HideAsync();
}
