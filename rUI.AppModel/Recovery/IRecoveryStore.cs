namespace rUI.AppModel.Recovery;

public interface IRecoveryStore<TState> where TState : class
{
    string Contract { get; }

    Task<RecoveryEnvelope<TState>?> LoadAsync(string scope, CancellationToken cancellationToken = default);
    Task SaveAsync(RecoveryEnvelope<TState> state, CancellationToken cancellationToken = default);
    Task ClearAsync(string scope, CancellationToken cancellationToken = default);
}
