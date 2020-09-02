using Microsoft.Xna.Framework;
using ProjectWarnerShared.Input;

namespace ProjectWarnerShared.Lib.State
{
    public abstract class StateMachineWithInputHandler<S, T, E> : StateMachine<S, T, E>, IPlatformInputHandler where S : StateBase<S, T, E>, IPlatformInputHandler
    {
        public StateMachineWithInputHandler(S InitialState, T Target) : base(InitialState, Target) { }

        public Vector2 GetInputVector()
        {
            return CurrentState.GetInputVector();
        }

        public bool ShouldAcceptInput()
        {
            return CurrentState.ShouldAcceptInput();
        }

        public bool WasJumpButtonPressed()
        {
            return CurrentState.WasJumpButtonPressed();
        }
    }
}
