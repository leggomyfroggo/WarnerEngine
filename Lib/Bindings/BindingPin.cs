using WarnerEngine.Lib.Entities;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.Bindings
{
    public class BindingPin : ABinding<IPinnable, IPinnable>
    {
        public BindingPin(IPinnable Left, IPinnable Right) : base(Left, Right) { }

        public static void Bind(IPinnable Left, IPinnable Right)
        {
            GameService.GetService<IBindingService>().AddBinding(new BindingPin(Left, Right));
        }

        public override bool ProcessBinding()
        {
            Right.GetBackingBox().TeleportCentered(Left.GetBackingBox().GetCenterPoint());
            return false;
        }

        public override void OnLeftRemoved() { }
    }
}
