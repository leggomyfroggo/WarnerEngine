using ProjectWarnerShared.Scenes;

namespace ProjectWarnerShared.Lib.Entities
{
    public interface ISceneEntity
    {
        void OnAdd(Scene ParentScene);
        void OnRemove(Scene ParentScene);
    }
}
