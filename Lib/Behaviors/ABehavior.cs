namespace WarnerEngine.Lib.Behaviors
{
    public abstract class ABehavior<TLinked> : IBehavior
    {
        public void Run(object Parent, object Child)
        {
            RunImplementation((TLinked)Parent, (TLinked)Child);
        }

        public abstract void RunImplementation(TLinked Parent, TLinked Child);
    }
}
