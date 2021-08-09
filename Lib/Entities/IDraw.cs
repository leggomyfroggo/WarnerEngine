namespace WarnerEngine.Lib.Entities
{
    public interface IDraw : IPositionable
    {
        void Draw();
        bool IsVisible();
    }
}
