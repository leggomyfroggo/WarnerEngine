using System;

namespace WarnerEngine.Lib.ECS.Components.State
{
    public abstract class AState<TState> : IState where TState : IState
    {
        public void OnEnterBase(UInt64 EntityID, IState PreviousState)
        {
            OnEnter(EntityID, (TState)PreviousState);
        }

        public abstract void OnEnter(UInt64 EntityID, TState PreviousState);

        public IState UpdateBase(UInt64 EntityID, float DT)
        {
            return Update(EntityID, DT);
        }

        public abstract TState Update(UInt64 EntityID, float DT);

        public void OnExitBase(UInt64 EntityID, IState IncomingState)
        {
            OnExit(EntityID, (TState)IncomingState);
        }

        public abstract void OnExit(UInt64 EntityID, TState IncomingState);

        public abstract IState HandleEvent(IEvent Event);
    }
}
