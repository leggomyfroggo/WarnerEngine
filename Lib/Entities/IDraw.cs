using WarnerEngine.Lib.Components;

namespace WarnerEngine.Lib.Entities
{
    public interface IDraw
    {
        void Draw();
        BackingBox GetBackingBox();
        bool IsVisible();
    }
}
