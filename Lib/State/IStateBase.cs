namespace WarnerEngine.Lib.State
{
    public interface IStateBase<S, T, E>
    {
        E GetStateType();
        void Enter(T Target, S PreviousState);
        S Update(T Target, float DT);
        void Exit(T Target, S IncomingState);
        S ConsiderStateChange(S CandidateState, T Target);
    }
}
