using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia;
using System.Windows.Input;

namespace rUI.Avalonia.Desktop.Controls;

public enum DialogResult
{
    None,
    Primary,
    Secondary,
    Close
}

public class ContentDialog : ContentControl
{
    private Border? _overlayPart;

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(Title));

    public static readonly StyledProperty<string?> PrimaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(PrimaryButtonText));

    public static readonly StyledProperty<string?> SecondaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(SecondaryButtonText));

    public static readonly StyledProperty<string?> CloseButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string?>(nameof(CloseButtonText));

    public static readonly StyledProperty<ICommand?> PrimaryButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(PrimaryButtonCommand));

    public static readonly StyledProperty<ICommand?> SecondaryButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(SecondaryButtonCommand));

    public static readonly StyledProperty<ICommand?> CloseButtonCommandProperty =
        AvaloniaProperty.Register<ContentDialog, ICommand?>(nameof(CloseButtonCommand));

    public static readonly StyledProperty<bool> IsPrimaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsPrimaryButtonEnabled), true);

    public static readonly StyledProperty<bool> IsSecondaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsSecondaryButtonEnabled), true);

    public static readonly StyledProperty<bool> IsCloseButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsCloseButtonEnabled), true);

    public static readonly StyledProperty<DefaultButton> DefaultButtonProperty =
        AvaloniaProperty.Register<ContentDialog, DefaultButton>(nameof(DefaultButton), DefaultButton.None);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsOpen), false);

    public static readonly StyledProperty<IBrush?> OverlayBrushProperty =
        AvaloniaProperty.Register<ContentDialog, IBrush?>(
            nameof(OverlayBrush),
            new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)));

    public static readonly StyledProperty<DialogResult> DialogResultProperty =
        AvaloniaProperty.Register<ContentDialog, DialogResult>(nameof(DialogResult), DialogResult.None);

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? PrimaryButtonText
    {
        get => GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    public string? SecondaryButtonText
    {
        get => GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    public string? CloseButtonText
    {
        get => GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    public ICommand? PrimaryButtonCommand
    {
        get => GetValue(PrimaryButtonCommandProperty);
        set => SetValue(PrimaryButtonCommandProperty, value);
    }

    public ICommand? SecondaryButtonCommand
    {
        get => GetValue(SecondaryButtonCommandProperty);
        set => SetValue(SecondaryButtonCommandProperty, value);
    }

    public ICommand? CloseButtonCommand
    {
        get => GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    public bool IsPrimaryButtonEnabled
    {
        get => GetValue(IsPrimaryButtonEnabledProperty);
        set => SetValue(IsPrimaryButtonEnabledProperty, value);
    }

    public bool IsSecondaryButtonEnabled
    {
        get => GetValue(IsSecondaryButtonEnabledProperty);
        set => SetValue(IsSecondaryButtonEnabledProperty, value);
    }

    public bool IsCloseButtonEnabled
    {
        get => GetValue(IsCloseButtonEnabledProperty);
        set => SetValue(IsCloseButtonEnabledProperty, value);
    }

    public DefaultButton DefaultButton
    {
        get => GetValue(DefaultButtonProperty);
        set => SetValue(DefaultButtonProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public IBrush? OverlayBrush
    {
        get => GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    public DialogResult DialogResult
    {
        get => GetValue(DialogResultProperty);
        set => SetValue(DialogResultProperty, value);
    }

    public event EventHandler<DialogResult>? Closed;

    // TODO: Check how to unregister events when control unloaded/destroyed
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _overlayPart = e.NameScope.Find<Border>("PART_Overlay");
        if (_overlayPart != null)
        {
            _overlayPart.PointerPressed += OnOverlayPointerPressed;
        }

        var primaryButton = e.NameScope.Find<Button>("PART_PrimaryButton");
        if (primaryButton != null)
        {
            primaryButton.Click += OnPrimaryButtonClick;
        }

        var secondaryButton = e.NameScope.Find<Button>("PART_SecondaryButton");
        if (secondaryButton != null)
        {
            secondaryButton.Click += OnSecondaryButtonClick;
        }

        var closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        if (closeButton != null)
        {
            closeButton.Click += OnCloseButtonClick;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape && IsOpen)
        {
            CloseDialog(DialogResult.None);
            e.Handled = true;
        }
    }

    private void OnOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CloseDialog(DialogResult.None);
    }

    private void OnPrimaryButtonClick(object? sender, RoutedEventArgs e)
    {
        PrimaryButtonCommand?.Execute(null);
        CloseDialog(DialogResult.Primary);
    }

    private void OnSecondaryButtonClick(object? sender, RoutedEventArgs e)
    {
        SecondaryButtonCommand?.Execute(null);
        CloseDialog(DialogResult.Secondary);
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        CloseButtonCommand?.Execute(null);
        CloseDialog(DialogResult.Close);
    }

    private void CloseDialog(DialogResult result)
    {
        DialogResult = result;
        IsOpen = false;
        Closed?.Invoke(this, result);
    }

    // TODO: Should Hide be here ??
    public Task HideAsync()
    {
        if (!IsOpen)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource();
        EventHandler<DialogResult>? handler = null;

        handler = (s, result) =>
        {
            Closed -= handler;
            tcs.SetResult();
        };

        Closed += handler;
        CloseDialog(DialogResult.None);

        return tcs.Task;
    }

    // TODO: Should Show be here ??
    public Task<DialogResult> ShowAsync()
    {
        var tcs = new TaskCompletionSource<DialogResult>();
        EventHandler<DialogResult>? handler = null;

        handler = (s, result) =>
        {
            Closed -= handler;
            tcs.SetResult(result);
        };

        Closed += handler;
        IsOpen = true;

        return tcs.Task;
    }
}

// TODO: Should this enum be here ??
public enum DefaultButton
{
    None,
    Primary,
    Secondary,
    Close
}
