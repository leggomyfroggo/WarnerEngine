using System;

namespace WarnerEngine.Lib.ECS
{
    public interface IEntity
    {
        UInt64 ID { get; }

        IEntity AddComponent(IComponent Component);
    }
}
