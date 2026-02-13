using rUI.Avalonia.Desktop.Controls;

namespace rUI.Avalonia.Desktop.Services;

public interface IOverlayService
{
    // TODO: What is this action here ?
    Task ShowAsync(Action<OverlayControl>? configure = null);
    // TODO: What is this action here ?
    void Update(Action<OverlayControl> configure);
    Task HideAsync();
}
