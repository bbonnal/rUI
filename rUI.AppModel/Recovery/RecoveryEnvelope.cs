namespace rUI.AppModel.Recovery;

public sealed record RecoveryEnvelope<TState>(
    string Contract,
    string Scope,
    DateTimeOffset CapturedAtUtc,
    TState Data);
