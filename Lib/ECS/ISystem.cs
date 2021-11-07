namespace WarnerEngine.Lib.ECS
{
    public interface ISystem
    {
        void Initialize();
        void Process();
    }
}
