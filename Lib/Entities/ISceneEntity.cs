namespace WarnerEngine.Lib.Entities
{
    public interface ISceneEntity
    {
        void OnAdd(Scene ParentScene);
        void OnRemove(Scene ParentScene);
    }
}
