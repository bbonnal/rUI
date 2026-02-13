using rUI.Avalonia.Desktop.Controls;

namespace rUI.Avalonia.Desktop.Services;

public interface IContentDialogService
{
    // TODO: Should there be a show message dialog?
    Task<DialogResult> ShowMessageAsync(string title, string message, string closeButtonText = "OK");
    Task<DialogResult> ShowAsync(Action<ContentDialog> configure);
    Task HideAsync();
}
