using System;
using System.Collections.Generic;
using System.Text;

namespace WarnerEngine.Services
{
    public abstract class Service : IService
    {
        public virtual void PreDraw(float DT) { }
        public virtual ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }
        public virtual void PostDraw() { }
        public abstract Type GetBackingInterfaceType();
    }
}
