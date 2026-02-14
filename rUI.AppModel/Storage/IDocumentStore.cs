using rUI.AppModel.Contracts;

namespace rUI.AppModel.Storage;

public interface IDocumentStore
{
    Task<string?> ReadAsync(PersistenceKey key, CancellationToken cancellationToken = default);
    Task WriteAsync(PersistenceKey key, string content, CancellationToken cancellationToken = default);
    Task DeleteAsync(PersistenceKey key, CancellationToken cancellationToken = default);
}
