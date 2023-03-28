using System;

namespace WarnerEngine.Lib.ECS
{
    public abstract class AComponent : IComponent
    {
        private UInt64? _parentID;
        public UInt64 ParentID
        {
            get
            {
                return _parentID.Value;
            }

            set
            {
                if (_parentID != null)
                {
                    throw new Exception("Parent ID has already been set for this component");
                }
                _parentID = value;
            }
        }
    }
}
