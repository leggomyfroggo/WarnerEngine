using ProjectWarnerShared.Lib.Components;
using ProjectWarnerShared.Scenes;

namespace ProjectWarnerShared.Lib.Entities
{
    public interface ISpawnableEntity : ISceneEntity
    {
        bool AttemptReap(Scene ParentScene);
        void Reap(Scene ParentScene);
        BackingBox GetBackingBox();
    }
}
