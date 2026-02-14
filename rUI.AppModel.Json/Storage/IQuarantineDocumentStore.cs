using rUI.AppModel.Contracts;

namespace rUI.AppModel.Json.Storage;

public interface IQuarantineDocumentStore
{
    Task QuarantineAsync(
        PersistenceKey key,
        string content,
        string reason,
        CancellationToken cancellationToken = default);
}
