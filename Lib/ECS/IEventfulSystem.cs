namespace WarnerEngine.Lib.ECS
{
    public interface IEventfulSystem : ISystem
    {
        void HandleEvent(IEvent Event);
    }
}
