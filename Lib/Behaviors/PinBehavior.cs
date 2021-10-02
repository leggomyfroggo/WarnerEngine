using WarnerEngine.Lib.Entities;

namespace WarnerEngine.Lib.Behaviors
{
    public class PinBehavior : ABehavior<IPinnable>
    {
        public override void RunImplementation(IPinnable Parent, IPinnable Child)
        {
            Child.GetBackingBox().TeleportCentered(Parent.GetBackingBox().GetCenterPoint());
        }
    }
}
