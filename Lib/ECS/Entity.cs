using System;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.ECS
{
    public sealed class Entity : IEntity
    {
        private readonly UInt64 _id;

        public UInt64 ID 
        { 
            get
            {
                return _id;
            } 
        }

        public Entity()
        {
            _id = GameService.GetService<IECSService>().GetNextID();
            GameService.GetService<IECSService>().RegisterEntity(this);
        }

        public IEntity AddComponent(IComponent Component)
        {
            GameService.GetService<IECSService>().RegisterComponent(ID, Component);
            Component.ParentID = ID;
            return this;
        }

        public TComponent GetComponent<TComponent>() where TComponent : IComponent
        {
            return GameService.GetService<IECSService>().GetComponent<TComponent>(ID);
        }
    }
}
