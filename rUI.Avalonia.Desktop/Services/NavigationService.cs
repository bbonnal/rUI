using Avalonia.Controls;
using rUI.Avalonia.Desktop.Controls.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;

namespace rUI.Avalonia.Desktop.Services;

/// <summary>
/// Implementation of the navigation service for managing application navigation state and lifecycle.
/// </summary>
/// <param name="items">The collection of main navigation items.</param>
/// <param name="footerItems">The collection of footer navigation items.</param>
public class NavigationService(
    IReadOnlyList<NavigationItemControl> items,
    IReadOnlyList<NavigationItemControl>? footerItems = null)
    : ObservableObject, INavigationService
{
    /// <summary>
    /// Semaphore used to synchronize navigation operations and prevent concurrent navigations.
    /// </summary>
    private readonly SemaphoreSlim _navigationLock = new(1, 1);

    /// <summary>
    /// Gets the currently displayed page.
    /// </summary>
    public Control? CurrentPage
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets the currently selected navigation item.
    /// Changing this property triggers navigation to the selected item.
    /// </summary>
    public NavigationItemControl? SelectedItem
    {
        get;
        set
        {
            var previousItem = field;
            if (SetProperty(ref field, value))
                _ = TryNavigateToItemAsync(value, previousItem);
        }
    }

    /// <summary>
    /// Gets the main navigation items.
    /// </summary>
    public IReadOnlyList<NavigationItemControl> Items { get; } = items;

    /// <summary>
    /// Gets the footer navigation items.
    /// </summary>
    public IReadOnlyList<NavigationItemControl>? FooterItems { get; } = footerItems;

    /// <summary>
    /// Navigates to a specific page control.
    /// </summary>
    /// <param name="page">The page control to navigate to.</param>
    /// <returns>A task representing the asynchronous navigation operation.</returns>
    public async Task NavigateToAsync(Control page)
    {
        if (!await _navigationLock.WaitAsync(0))
            return;

        try
        {
            if (CurrentPage is not null)
            {
                var allowed = await InvokeDisappearingAsync(CurrentPage);
                if (!allowed)
                    return; // Navigation cancelled
            }

            CurrentPage = page;

            SelectedItem = FindItemForPage(page);

            await InvokeAppearingAsync(CurrentPage);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    /// <summary>
    /// Finds the navigation item that corresponds to the given page control.
    /// </summary>
    /// <param name="page">The page control to find an item for.</param>
    /// <returns>The matching <see cref="NavigationItemControl"/> if found; otherwise, <c>null</c>.</returns>
    private NavigationItemControl? FindItemForPage(Control page)
    {
        var pageType = page.GetType();

        foreach (var item in Items)
        {
            if (item.PageType == pageType)
                return item;
        }

        if (FooterItems != null)
        {
            foreach (var item in FooterItems)
            {
                if (item.PageType == pageType)
                    return item;
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to navigate to the specified navigation item.
    /// </summary>
    /// <param name="targetItem">The target navigation item.</param>
    /// <param name="previousItem">The previously selected navigation item, to be restored if navigation is cancelled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task TryNavigateToItemAsync(NavigationItemControl? targetItem, NavigationItemControl? previousItem)
    {
        // Prevent concurrent navigations
        if (!await _navigationLock.WaitAsync(0))
            return;

        try
        {
            if (CurrentPage is not null)
            {
                var allowed = await InvokeDisappearingAsync(CurrentPage);

                if (!allowed)
                {
                    // Navigation canceled - restore previous selection
                    SelectedItem = previousItem;
                    return;
                }
            }

            CurrentPage = targetItem?.Factory?.Invoke();

            // Call OnAppearing on the new page (async)
            if (CurrentPage is not null)
                await InvokeAppearingAsync(CurrentPage);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    /// <summary>
    /// Invokes the <see cref="INavigationViewModel.OnDisappearingAsync"/> method on the page's data context if available.
    /// </summary>
    /// <param name="page">The page control that is disappearing.</param>
    /// <returns>A task representing the asynchronous operation, returning <c>true</c> if navigation is allowed; otherwise, <c>false</c>.</returns>
    private async Task<bool> InvokeDisappearingAsync(Control page)
    {
        try
        {
            if (page.DataContext is INavigationViewModel nav)
                return await nav.OnDisappearingAsync();
            return true;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Invokes the <see cref="INavigationViewModel.OnAppearingAsync"/> method on the page's data context if available.
    /// </summary>
    /// <param name="page">The page control that has appeared.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task InvokeAppearingAsync(Control page)
    {
        try
        {
            if (page.DataContext is INavigationViewModel nav)
                await nav.OnAppearingAsync();
        }
        catch
        {
            // ignored
        }
    }
}