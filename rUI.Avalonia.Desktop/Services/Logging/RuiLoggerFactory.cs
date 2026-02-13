using Microsoft.Extensions.Logging;

namespace rUI.Avalonia.Desktop.Services.Logging;

public sealed class RuiLoggerFactory : IRuiLoggerFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public RuiLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public IRuiLogger CreateLogger<TCategoryName>()
    {
        return new RuiLogger(_loggerFactory.CreateLogger<TCategoryName>());
    }

    public IRuiLogger CreateLogger(string categoryName)
    {
        return new RuiLogger(_loggerFactory.CreateLogger(categoryName));
    }
}
