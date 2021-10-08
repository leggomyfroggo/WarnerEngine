using System;
using System.Collections.Generic;

using WarnerEngine.Lib.ECS;

namespace WarnerEngine.Services.Implementations
{
    public class ECSService : IECSService
    {
        public const UInt64 NULL_ID = 0;

        private UInt64 _nextID;

        private Dictionary<UInt64, IEntity> _entities;
        private Dictionary<Type, Dictionary<UInt64, IComponent>> _components;
        private List<ISystem> _systems;

        public Type GetBackingInterfaceType()
        {
            return typeof(IECSService);
        }

        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>() { typeof(IEventService) };
        }

        public void Initialize()
        {
            _nextID = 1;
            _entities = new Dictionary<UInt64, IEntity>();
            _components = new Dictionary<Type, Dictionary<UInt64, IComponent>>();
            _systems = new List<ISystem>();
        }

        public void PreDraw(float DT)
        {
            foreach (ISystem system in _systems)
            {
                system.Process();
            }
        }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw()
        {
            // TODO: Something about systems updates
        }

        public UInt64 GetNextID()
        {
            return _nextID++;
        }

        public IECSService RegisterEntity(IEntity Entity)
        {
            _entities[Entity.ID] = Entity;
            return this;
        }

        public IECSService DeregisterEntity(IEntity Entity)
        {
            _entities.Remove(Entity.ID);
            return this;
        }

        public IECSService DeregisterEntity(UInt64 ID)
        {
            _entities.Remove(ID);
            return this;
        }

        public IEntity GetEntity(UInt64 ID)
        {
            _entities.TryGetValue(ID, out IEntity entity);
            return entity;
        }

        public IEntity GetxEntity(UInt64 ID)
        {
            return _entities[ID];
        }

        public IECSService RegisterComponent(UInt64 EntityID, IComponent Component)
        {
            Type t = Component.GetType();
            _components[t] = _components[t] ?? new Dictionary<UInt64, IComponent>();
            _components[t].Add(EntityID, Component);
            return this;
        }

        public IECSService DeregisterComponent<TComponent>(UInt64 EntityID)
        {
            _components[typeof(TComponent)].Remove(EntityID);
            return this;
        }

        public IEnumerable<KeyValuePair<UInt64, TComponent>> GetComponentEnumerator<TComponent>() where TComponent : IComponent
        {
            Type t = typeof(TComponent);
            return _components[t] != null ? (IEnumerable<KeyValuePair<UInt64, TComponent>>)_components[t] : new Dictionary<UInt64, TComponent>();
        }

        public TComponent GetComponent<TComponent>(UInt64 EntityID) where TComponent : IComponent
        {
            return (TComponent)_components[typeof(TComponent)][EntityID];
        }

        public IEnumerable<UInt64> GetEntitiesWithComponent<TComponent>() where TComponent : IComponent
        {
            return _components[typeof(TComponent)].Keys;
        }

        public IECSService RegisterSystem(ISystem System)
        {
            _systems.Add(System);
            return this;
        }
    }
}
