using CommunityToolkit.Mvvm.ComponentModel;
using rUI.Avalonia.Desktop;
using rUI.Avalonia.Desktop.Controls.Navigation;

namespace rUI.Avalonia.Desktop.Services;

/// <summary>
/// Navigation service using type-based ViewModel resolution via IServiceProvider.
/// </summary>
public class NavigationService(IServiceProvider serviceProvider) : ObservableObject, INavigationService
{
    private readonly SemaphoreSlim _navigationLock = new(1, 1);

    public object? CurrentPage
    {
        get;
        set => SetProperty(ref field, value);
    }

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

    public IReadOnlyList<NavigationItemControl> Items { get; private set; } = [];

    public IReadOnlyList<NavigationItemControl>? FooterItems { get; private set; }

    public void Initialize(IReadOnlyList<NavigationItemControl> items, IReadOnlyList<NavigationItemControl>? footerItems = null)
    {
        Items = items;
        FooterItems = footerItems;
    }

    public Task NavigateToAsync<TViewModel>() where TViewModel : class
        => NavigateToAsync(typeof(TViewModel));

    public async Task NavigateToAsync(Type viewModelType)
    {
        if (!await _navigationLock.WaitAsync(0))
            return;

        try
        {
            if (CurrentPage is not null)
            {
                var allowed = await InvokeDisappearingAsync(CurrentPage);
                if (!allowed)
                    return;
            }

            CurrentPage = ResolveViewModel(viewModelType);
            SelectedItem = FindItemForViewModel(viewModelType);

            if (CurrentPage is not null)
                await InvokeAppearingAsync(CurrentPage);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    private async Task TryNavigateToItemAsync(NavigationItemControl? targetItem, NavigationItemControl? previousItem)
    {
        if (!await _navigationLock.WaitAsync(0))
            return;

        try
        {
            if (CurrentPage is not null)
            {
                var allowed = await InvokeDisappearingAsync(CurrentPage);
                if (!allowed)
                {
                    SelectedItem = previousItem;
                    return;
                }
            }

            CurrentPage = targetItem?.PageViewModelType is { } vmType
                ? ResolveViewModel(vmType)
                : null;

            if (CurrentPage is not null)
                await InvokeAppearingAsync(CurrentPage);
        }
        finally
        {
            _navigationLock.Release();
        }
    }

    private object ResolveViewModel(Type viewModelType)
        => serviceProvider.GetService(viewModelType)
           ?? throw new InvalidOperationException($"ViewModel type '{viewModelType.FullName}' is not registered in DI.");

    private NavigationItemControl? FindItemForViewModel(Type viewModelType)
    {
        foreach (var item in Items)
        {
            if (item.PageViewModelType == viewModelType)
                return item;
        }

        if (FooterItems is null)
            return null;

        foreach (var item in FooterItems)
        {
            if (item.PageViewModelType == viewModelType)
                return item;
        }

        return null;
    }

    private static async Task<bool> InvokeDisappearingAsync(object viewModel)
    {
        try
        {
            if (viewModel is INavigationViewModel nav)
                return await nav.OnDisappearingAsync();
            return true;
        }
        catch
        {
            return true;
        }
    }

    private static async Task InvokeAppearingAsync(object viewModel)
    {
        try
        {
            if (viewModel is INavigationViewModel nav)
                await nav.OnAppearingAsync();
        }
        catch
        {
            // ignored
        }
    }
}
