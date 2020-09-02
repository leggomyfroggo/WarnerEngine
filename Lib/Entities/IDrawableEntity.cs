using ProjectWarnerShared.Lib.Components;

namespace ProjectWarnerShared.Lib.Entities
{
    public interface IDrawableEntity : ISceneEntity, IPreDraw, IDraw, IPostDraw
    {
    }
}
