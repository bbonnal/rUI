using System;
using System.Threading;
using System.Threading.Tasks;
using rUIAppModelTester.AppModel;

namespace rUIAppModelTester.Services;

public interface IAppSettingsRuntime
{
    AppModelTesterSettings Current { get; }
    bool IsInitialized { get; }
    DateTimeOffset? LastLoadedAtUtc { get; }
    DateTimeOffset? LastSavedAtUtc { get; }
    string SettingsFilePath { get; }

    event EventHandler? Changed;

    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppModelTesterSettings settings, CancellationToken cancellationToken = default);
    Task ResetToDefaultsAsync(CancellationToken cancellationToken = default);
    Task DeletePersistedAsync(CancellationToken cancellationToken = default);
    Task ReloadAsync(CancellationToken cancellationToken = default);
}
