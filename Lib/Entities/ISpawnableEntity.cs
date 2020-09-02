using WarnerEngine.Lib.Components;

namespace WarnerEngine.Lib.Entities
{
    public interface ISpawnableEntity : ISceneEntity
    {
        bool AttemptReap(Scene ParentScene);
        void Reap(Scene ParentScene);
        BackingBox GetBackingBox();
    }
}
