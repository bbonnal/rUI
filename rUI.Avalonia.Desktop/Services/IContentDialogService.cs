using rUI.Avalonia.Desktop.Controls;

namespace rUI.Avalonia.Desktop.Services;

public interface IContentDialogService
{
    void RegisterHost(ContentDialog dialog);
    Task<DialogResult> ShowMessageAsync(string title, string message, string closeButtonText = "OK");
    Task<DialogResult> ShowAsync(Action<ContentDialog> configure);
    Task HideAsync();
}
