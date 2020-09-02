namespace WarnerEngine.Lib.Components
{
    public interface IComponent
    {
        void PreDraw(float DT);
        void PostDraw();
    }
}
