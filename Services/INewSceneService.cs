using WarnerEngine.Lib.SceneGraph;

namespace WarnerEngine.Services
{
    public interface INewSceneService : IService
    {
        IScene CurrentScene { get; }

        INewSceneService RegisterScene(IScene Scene);
        INewSceneService SetScene(string SceneName);
    }
}
