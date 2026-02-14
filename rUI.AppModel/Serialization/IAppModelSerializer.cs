namespace rUI.AppModel.Serialization;

public interface IAppModelSerializer
{
    string Serialize<T>(T value);
    T Deserialize<T>(string payload);
}
