using WarnerEngine.Lib;

namespace WarnerEngine.Services
{
    public interface ISceneService : IService
    {
        Scene CurrentScene { get; }

        ISceneService RegisterScene(string Key, Scene L);
        ISceneService ResetCurrentScene();
        ISceneService SetScene(string Key);
    }
}
