using System;
using Avalonia.Controls;
using rUIAvaloniaDesktopTester.ViewModels;

namespace rUIAvaloniaDesktopTester.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MainWindowViewModel vm)
        {
            vm.DialogService.RegisterHost(HostDialog);
            vm.OverlayService.RegisterHost(HostOverlay);
            vm.InfoBarService.RegisterHost(HostInfoBar);
        }
    }
}