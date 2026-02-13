namespace rUI.Avalonia.Desktop;

/// <summary>
/// Resolves a navigation target ViewModel from its type.
/// </summary>
public interface INavigationViewModelResolver
{
    /// <summary>
    /// Resolves a ViewModel instance for navigation.
    /// </summary>
    /// <param name="viewModelType">The ViewModel type to resolve.</param>
    /// <returns>The resolved ViewModel instance.</returns>
    object Resolve(Type viewModelType);
}
