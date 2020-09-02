namespace WarnerEngine.Lib.Input
{
    public interface IPlatformInputHandlerWithTarget<T> : IPlatformInputHandler
    {
        void SetTarget(T Target);
    }
}
