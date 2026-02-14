using System.Text;
using rUI.AppModel.Contracts;
using rUI.AppModel.Storage;

namespace rUI.AppModel.Json.Storage;

public sealed class FileDocumentStore(FileDocumentStoreOptions options) : IDocumentStore, IQuarantineDocumentStore
{
    private const string TempExtension = ".tmp";

    public async Task<string?> ReadAsync(PersistenceKey key, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);
        if (!File.Exists(path))
            return null;

        return await File.ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
    }

    public async Task WriteAsync(PersistenceKey key, string content, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var tempPath = $"{path}{TempExtension}";
        await File.WriteAllTextAsync(tempPath, content, Encoding.UTF8, cancellationToken);
        File.Move(tempPath, path, overwrite: true);
    }

    public Task DeleteAsync(PersistenceKey key, CancellationToken cancellationToken = default)
    {
        var path = ResolvePath(key);
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public async Task QuarantineAsync(
        PersistenceKey key,
        string content,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var quarantineRoot = Path.Combine(options.RootDirectory, options.QuarantineDirectoryName);
        Directory.CreateDirectory(quarantineRoot);

        var safeName = KeyPath.NormalizeSegment(key.Value);
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmssfff");

        var payloadPath = Path.Combine(quarantineRoot, $"{safeName}-{timestamp}{options.DefaultExtension}");
        var metaPath = Path.Combine(quarantineRoot, $"{safeName}-{timestamp}.meta.txt");

        await File.WriteAllTextAsync(payloadPath, content, Encoding.UTF8, cancellationToken);
        await File.WriteAllTextAsync(metaPath, reason, Encoding.UTF8, cancellationToken);
        await DeleteAsync(key, cancellationToken);
    }

    private string ResolvePath(PersistenceKey key)
    {
        var relative = KeyPath.NormalizeRelativePath(key.Value, options.DefaultExtension);
        return Path.Combine(options.RootDirectory, relative);
    }
}

internal static class KeyPath
{
    public static string NormalizeRelativePath(string value, string defaultExtension)
    {
        var parts = value
            .Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizeSegment)
            .ToArray();

        var relative = Path.Combine(parts);
        if (!relative.EndsWith(defaultExtension, StringComparison.OrdinalIgnoreCase))
            relative += defaultExtension;

        return relative;
    }

    public static string NormalizeSegment(string input)
    {
        var chars = input.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '_').ToArray();
        var candidate = new string(chars).Trim('_');
        return string.IsNullOrWhiteSpace(candidate) ? "default" : candidate;
    }
}
