namespace rUI.Avalonia.Desktop.Services.Shortcuts;

public interface IShortcutBindingProvider
{
    IEnumerable<ShortcutDefinition> GetShortcutDefinitions();
}
