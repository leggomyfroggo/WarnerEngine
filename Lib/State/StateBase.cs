namespace WarnerEngine.Lib.State
{
    public abstract class StateBase<S, T, E> : IStateBase<S, T, E>
    {
        public abstract S ConsiderStateChange(S CandidateState, T Target);

        public abstract void Enter(T Target, S PreviousState);

        public abstract void Exit(T Target, S IncomingState);

        public abstract E GetStateType();

        public abstract S Update(T Target, float DT);
    }
}
