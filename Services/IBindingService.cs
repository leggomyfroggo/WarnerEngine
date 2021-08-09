using WarnerEngine.Lib.Bindings;
using WarnerEngine.Lib.Entities;

namespace WarnerEngine.Services
{
    public interface IBindingService : IService
    {
        void AddBinding(IBinding Binding);
        void RemoveBindingsForEntity(ISceneEntity Entity);
    }
}
