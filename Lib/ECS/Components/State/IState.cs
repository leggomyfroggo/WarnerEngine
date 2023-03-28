using System;

namespace WarnerEngine.Lib.ECS.Components.State
{
    public interface IState
    {
        void OnEnterBase(UInt64 EntityID, IState PreviousState);
        IState UpdateBase(UInt64 EntityID, float DT);
        void OnExitBase(UInt64 EntityID, IState IncomingState);
        IState HandleEvent(IEvent Event);
    }
}
