using System;
using System.Collections.Generic;

using WarnerEngine.Lib.ECS;

namespace WarnerEngine.Services
{
    public interface IECSService : IService
    {
        UInt64 GetNextID();

        IECSService RegisterEntity(IEntity Entity);
        IECSService DeregisterEntity(IEntity Entity);
        IECSService DeregisterEntity(UInt64 ID);
        IEntity GetEntity(UInt64 ID);
        IEntity GetxEntity(UInt64 ID);

        IECSService RegisterComponent(UInt64 EntityID, IComponent Component);
        IECSService DeregisterComponent<TComponent>(UInt64 EntityID);
        TComponent GetComponent<TComponent>(UInt64 EntityID) where TComponent : IComponent;
        IEnumerable<UInt64> GetEntitiesWithComponent<TComponent>() where TComponent : IComponent;

        IECSService RegisterSystem(ISystem System);

        IECSService SubscribeToEventType(Type EventType, IEventfulSystem System);
        IECSService RaiseEvent<TEvent>(TEvent Event) where TEvent : IEvent;
    }
}
