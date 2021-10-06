using System;

namespace WarnerEngine.Lib.ECS
{
    public abstract class AComponent : IComponent
    {
        private UInt64 _parentID;
        public UInt64 ParentID
        {
            get
            {
                return _parentID;
            }
        }

        public AComponent(UInt64 ParentID)
        {
            _parentID = ParentID;
        }
    }
}
