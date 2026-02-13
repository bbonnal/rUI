namespace rUI.Avalonia.Desktop.Services.Logging;

public interface IRuiLogger
{
    IDisposable BeginScope(IReadOnlyDictionary<string, object?> properties);
    void Trace(string messageTemplate, params object?[] args);
    void Debug(string messageTemplate, params object?[] args);
    void Information(string messageTemplate, params object?[] args);
    void Warning(string messageTemplate, params object?[] args);
    void Error(string messageTemplate, params object?[] args);
    void Error(Exception exception, string messageTemplate, params object?[] args);
    void Critical(string messageTemplate, params object?[] args);
    void Critical(Exception exception, string messageTemplate, params object?[] args);
}

public interface IRuiLogger<TCategoryName> : IRuiLogger;
