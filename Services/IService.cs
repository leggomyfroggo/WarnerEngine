using System;
using System.Collections.Generic;

namespace WarnerEngine.Services
{
    public interface IService
    {
        HashSet<Type> GetDependencies();
        void Initialize();
        void PreDraw(float DT);
        ServiceCompositionMetadata Draw();
        void PostDraw();
        Type GetBackingInterfaceType();
    }
}
