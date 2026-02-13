namespace rUI.Avalonia.Desktop.Services.Logging;

public interface IRuiLoggerFactory
{
    IRuiLogger CreateLogger<TCategoryName>();
    IRuiLogger CreateLogger(string categoryName);
}
