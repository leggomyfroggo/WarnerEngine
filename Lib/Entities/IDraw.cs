using ProjectWarnerShared.Lib.Components;

namespace ProjectWarnerShared.Lib.Entities
{
    public interface IDraw
    {
        void Draw();
        BackingBox GetBackingBox();
        bool IsVisible();
    }
}
