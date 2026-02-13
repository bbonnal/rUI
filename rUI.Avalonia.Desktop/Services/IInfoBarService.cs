using rUI.Avalonia.Desktop.Controls;

namespace rUI.Avalonia.Desktop.Services;

public interface IInfoBarService
{
    void RegisterHost(InfoBarControl infoBar);
    // TODO: What is this action here ?
    Task ShowAsync(Action<InfoBarControl>? configure = null);
    Task HideAsync();
}
