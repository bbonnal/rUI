namespace rUI.AppModel.Json.Storage;

public sealed record FileDocumentStoreOptions(
    string RootDirectory,
    string QuarantineDirectoryName = "_quarantine",
    string DefaultExtension = ".json");
