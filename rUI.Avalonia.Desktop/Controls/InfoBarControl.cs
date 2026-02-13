using Avalonia.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;

namespace rUI.Avalonia.Desktop.Controls;

public enum InfoBarSeverity
{
    Info,
    Success,
    Warning,
    Error
}

public class InfoBarControl : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<InfoBarControl, string?>(nameof(Title));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<InfoBarControl, string?>(nameof(Message));

    public static readonly StyledProperty<InfoBarSeverity> SeverityProperty =
        AvaloniaProperty.Register<InfoBarControl, InfoBarSeverity>(nameof(Severity), InfoBarSeverity.Info);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<InfoBarControl, bool>(nameof(IsOpen), false);

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public InfoBarSeverity Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public event EventHandler? Closed;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var closeButton = e.NameScope.Find<Button>("PART_CloseButton");
        if (closeButton != null)
            closeButton.Click += OnCloseButtonClick;
    }

    // TODO: Should those method be here ??
    
    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    public void Close()
    {
        IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public Task CloseAsync()
    {
        Close();
        return Task.CompletedTask;
    }

    public Task ShowAsync()
    {
        var tcs = new TaskCompletionSource();
        EventHandler? handler = null;

        handler = (s, e) =>
        {
            Closed -= handler;
            tcs.SetResult();
        };

        Closed += handler;
        IsOpen = true;

        return tcs.Task;
    }
}
