using System;
using System.Collections.Generic;

using WarnerEngine.Lib.ECS.Components.State;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.ECS.Systems.State
{
    public abstract class AStateMachine<TState> : APollingSystem<Stateful<TState>>, IEventfulSystem where TState : IState
    {
        public override void Initialize()
        {
            var ecsService = GameService.GetService<IECSService>();
            foreach (Type type in GetDesiredEventTypes())
            {
                ecsService.SubscribeToEventType(type, this);
            }
        }

        // TODO: Get DT piped in here somehow
        public override void ProcessImplementation(IEnumerable<UInt64> Entities1)
        {
            foreach (UInt64 entityID in Entities1)
            {
                var statefulComponent = GameService.GetService<IECSService>().GetComponent<Stateful<TState>>(entityID);
                var currentState = statefulComponent.State;
                var maybeNewState = currentState.UpdateBase(entityID, 0.0167f);
                if (maybeNewState != currentState)
                {
                    currentState.OnExitBase(entityID, maybeNewState);
                    statefulComponent.State = maybeNewState;
                    maybeNewState.OnEnterBase(entityID, currentState);
                }
            }
        }

        public void HandleEvent(IEvent Event)
        {
            var entities = GameService.GetService<IECSService>().GetEntitiesWithComponent<Stateful<TState>>();
            foreach (var entityID in entities)
            {
                var statefulComponent = GameService.GetService<IECSService>().GetComponent<Stateful<TState>>(entityID);
                var currentState = statefulComponent.State;
                var maybeNewState = currentState.HandleEvent(Event);
                if (maybeNewState != currentState)
                {
                    currentState.OnExitBase(entityID, maybeNewState);
                    statefulComponent.State = maybeNewState;
                    maybeNewState.OnEnterBase(entityID, currentState);
                }
            }
        }

        public abstract HashSet<Type> GetDesiredEventTypes();
    }
}
