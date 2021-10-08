using System;
using System.Collections.Generic;

using WarnerEngine.Lib.ECS;
using WarnerEngine.Services;

namespace WarnerEngine.Lib.SceneGraph
{
    public class Node : INode
    {
        public UInt64 EntityID { get; private set; }
        public IEntity Entity
        {
            get
            {
                return GameService.GetService<IECSService>().GetEntity(EntityID);
            }
        }

        private Dictionary<UInt64, INode> _children;
        public IEnumerable<KeyValuePair<UInt64, INode>> Children
        {
            get
            {
                return _children;
            }
        }

        public Node(UInt64 EntityID)
        {
            this.EntityID = EntityID;
            _children = new Dictionary<UInt64, INode>();
        }

        public INode AddChild(UInt64 EntityID)
        {
            _children[EntityID] = new Node(EntityID);
            return this;
        }

        public INode RemoveChild(UInt64 EntityID)
        {
            _children.Remove(EntityID);
            return this;
        }
    }
}
