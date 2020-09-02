using WarnerEngine.Lib.Components;

namespace WarnerEngine.Lib.Entities
{
    public interface IDrawableEntity : ISceneEntity, IPreDraw, IDraw, IPostDraw
    {
    }
}
