namespace WarnerEngine.Lib
{
    public abstract class InteractionWOContext<TActor, TReceiver> : Interaction<TActor, TReceiver, bool> where TActor : class where TReceiver : class
    {
        protected override bool InitialAccumulatorValue => false;

        protected override sealed bool CanPerformAction(TActor Actor, bool Accumulator)
        {
            return CanPerformActionWOContext(Actor);
        }

        protected abstract bool CanPerformActionWOContext(TActor Actor);

        protected override sealed bool PerformAction(TActor Actor, TReceiver Receiver, bool Accumulator)
        {
            PerformActionWOContext(Actor, Receiver);
            return Accumulator;
        }

        protected abstract void PerformActionWOContext(TActor Actor, TReceiver Receiver);
    }
}
