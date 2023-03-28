using System;

namespace WarnerEngine.Lib.ECS
{
    public interface IComponent
    {
        UInt64 ParentID { get; set; }
    }
}
