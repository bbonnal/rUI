using Microsoft.Extensions.Logging;

namespace rUI.Avalonia.Desktop.Services.Logging;

public sealed class RuiLogger : IRuiLogger
{
    private readonly ILogger _logger;

    public RuiLogger(ILogger logger)
    {
        _logger = logger;
    }

    public IDisposable BeginScope(IReadOnlyDictionary<string, object?> properties)
    {
        return _logger.BeginScope(properties) ?? NoopScope.Instance;
    }

    public void Trace(string messageTemplate, params object?[] args)
    {
        _logger.LogTrace(messageTemplate, args);
    }

    public void Debug(string messageTemplate, params object?[] args)
    {
        _logger.LogDebug(messageTemplate, args);
    }

    public void Information(string messageTemplate, params object?[] args)
    {
        _logger.LogInformation(messageTemplate, args);
    }

    public void Warning(string messageTemplate, params object?[] args)
    {
        _logger.LogWarning(messageTemplate, args);
    }

    public void Error(string messageTemplate, params object?[] args)
    {
        _logger.LogError(messageTemplate, args);
    }

    public void Error(Exception exception, string messageTemplate, params object?[] args)
    {
        _logger.LogError(exception, messageTemplate, args);
    }

    public void Critical(string messageTemplate, params object?[] args)
    {
        _logger.LogCritical(messageTemplate, args);
    }

    public void Critical(Exception exception, string messageTemplate, params object?[] args)
    {
        _logger.LogCritical(exception, messageTemplate, args);
    }

    private sealed class NoopScope : IDisposable
    {
        public static readonly NoopScope Instance = new();

        public void Dispose()
        {
        }
    }
}

public sealed class RuiLogger<TCategoryName> : IRuiLogger<TCategoryName>
{
    private readonly RuiLogger _inner;

    public RuiLogger(ILogger<TCategoryName> logger)
    {
        _inner = new RuiLogger(logger);
    }

    public IDisposable BeginScope(IReadOnlyDictionary<string, object?> properties)
    {
        return _inner.BeginScope(properties);
    }

    public void Trace(string messageTemplate, params object?[] args)
    {
        _inner.Trace(messageTemplate, args);
    }

    public void Debug(string messageTemplate, params object?[] args)
    {
        _inner.Debug(messageTemplate, args);
    }

    public void Information(string messageTemplate, params object?[] args)
    {
        _inner.Information(messageTemplate, args);
    }

    public void Warning(string messageTemplate, params object?[] args)
    {
        _inner.Warning(messageTemplate, args);
    }

    public void Error(string messageTemplate, params object?[] args)
    {
        _inner.Error(messageTemplate, args);
    }

    public void Error(Exception exception, string messageTemplate, params object?[] args)
    {
        _inner.Error(exception, messageTemplate, args);
    }

    public void Critical(string messageTemplate, params object?[] args)
    {
        _inner.Critical(messageTemplate, args);
    }

    public void Critical(Exception exception, string messageTemplate, params object?[] args)
    {
        _inner.Critical(exception, messageTemplate, args);
    }
}
