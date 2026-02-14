using System.Text.Json;
using rUI.AppModel.Serialization;

namespace rUI.AppModel.Json.Serialization;

public sealed class SystemTextJsonAppModelSerializer(JsonSerializerOptions? options = null) : IAppModelSerializer
{
    private readonly JsonSerializerOptions _options = options ?? new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _options);
    }

    public T Deserialize<T>(string payload)
    {
        return JsonSerializer.Deserialize<T>(payload, _options)
               ?? throw new InvalidOperationException($"Deserialization returned null for '{typeof(T).FullName}'.");
    }
}
