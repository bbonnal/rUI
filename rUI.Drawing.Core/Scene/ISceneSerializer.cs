namespace rUI.Drawing.Core.Scene;

public interface ISceneSerializer
{
    string Serialize(SceneDocument scene);

    SceneDocument Deserialize(string json);
}
