using WarnerEngine.Lib.Entities;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.Bindings
{
    public class BindingRemoval : ABinding<ISceneEntity, ISceneEntity>
    {
        public BindingRemoval(ISceneEntity Left, ISceneEntity Right) : base(Left, Right) { }

        public static void Bind(ISceneEntity Left, ISceneEntity Right)
        {
            GameService.GetService<IBindingService>().AddBinding(new BindingRemoval(Left, Right));
        }

        public override bool ProcessBinding()
        {
            return false;
        }

        public override void OnLeftRemoved() 
        {
            GameService.GetService<ISceneService>().CurrentScene.RemoveEntity(Right);
        }
    }
}
