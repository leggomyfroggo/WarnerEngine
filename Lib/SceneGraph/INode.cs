using System;
using System.Collections.Generic;

using WarnerEngine.Lib.ECS;

namespace WarnerEngine.Lib.SceneGraph
{
    public interface INode
    {
        UInt64 EntityID { get; }
        IEntity Entity { get; }

        IEnumerable<KeyValuePair<UInt64, INode>> Children { get; }

        // TODO: Add a reference to the node's parent

        INode AddChild(UInt64 EntityID);
        INode RemoveChild(UInt64 EntityID);
    }
}
