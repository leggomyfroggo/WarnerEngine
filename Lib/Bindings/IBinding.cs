using WarnerEngine.Lib.Entities;

namespace WarnerEngine.Lib.Bindings
{
    public interface IBinding
    {
        ISceneEntity GetLeftEntity();
        ISceneEntity GetRightEntity();
        bool ProcessBinding();
        void OnLeftRemoved();
    }
}
