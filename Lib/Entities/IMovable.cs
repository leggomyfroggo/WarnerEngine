namespace WarnerEngine.Lib.Entities
{
    public interface IMovable : IDraw
    {
        bool IsMoving();
        bool DidContactThisFrame();
        void QueueContactSound(string Key, bool ShouldOverride = false);
    }
}
