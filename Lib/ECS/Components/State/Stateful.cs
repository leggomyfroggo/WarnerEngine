using System;

namespace WarnerEngine.Lib.ECS.Components.State
{
    public class Stateful<TState> : AComponent where TState : IState
    {
        public IState State { get; set; }

        public Stateful(TState InitialState)
        {
            State = InitialState;
        }
    }
}
