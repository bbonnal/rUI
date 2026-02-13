using Avalonia.Controls;

namespace rUI.Avalonia.Desktop;

/// <summary>
/// Defines a service for managing navigation within the application.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the currently displayed page.
    /// </summary>
    Control? CurrentPage { get; }

    /// <summary>
    /// Navigates to the specified page asynchronously.
    /// </summary>
    /// <param name="page">The control to navigate to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task NavigateToAsync(Control page);
}
