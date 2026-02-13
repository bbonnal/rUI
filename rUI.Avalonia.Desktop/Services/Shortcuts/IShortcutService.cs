using Avalonia.Controls;

namespace rUI.Avalonia.Desktop.Services.Shortcuts;

public interface IShortcutService
{
    IDisposable Bind(Control scope, IEnumerable<ShortcutDefinition> definitions);
}
