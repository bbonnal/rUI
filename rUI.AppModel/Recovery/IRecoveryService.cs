namespace rUI.AppModel.Recovery;

public interface IRecoveryService<TState> where TState : class
{
    Task<TState?> TryRestoreAsync(string scope, CancellationToken cancellationToken = default);
    Task CaptureAsync(string scope, TState state, CancellationToken cancellationToken = default);
    Task ClearAsync(string scope, CancellationToken cancellationToken = default);
}
