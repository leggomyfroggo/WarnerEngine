using System.Collections.Generic;

namespace WarnerEngine.Lib.State
{
    public abstract class StateMachine<S, T, E> where S : StateBase<S, T, E>
    {
        public S CurrentState;

        private Stack<S> stateStack;

        public StateMachine(S InitialState, T Target)
        {
            CurrentState = InitialState;
            CurrentState.Enter(Target, null);
            stateStack = new Stack<S>();
        }

        public void Update(T Target, float DT)
        {
            S stateUpdate = CurrentState.Update(Target, DT);
            if (stateUpdate != CurrentState)
            {
                var prevState = CurrentState;
                CurrentState.Exit(Target, stateUpdate);
                CurrentState = stateUpdate;
                CurrentState.Enter(Target, prevState);
            }
        }

        public void ConsiderStateChange(S CandidateState, T Target)
        {
            S stateUpdate = CurrentState.ConsiderStateChange(CandidateState, Target);
            if (stateUpdate != CurrentState)
            {
                var prevState = CurrentState;
                CurrentState.Exit(Target, stateUpdate);
                CurrentState = stateUpdate;
                CurrentState.Enter(Target, prevState);
            }
        }

        public E GetCurrentStateType()
        {
            return CurrentState.GetStateType();
        }

        public void PushState(S State)
        {
            stateStack.Push(State);
        }

        public S PopState()
        {
            return stateStack.Pop();
        }
    }
}
