using System;

namespace WarnerEngine.Services
{
    public interface IService
    {
        void PreDraw(float DT);
        ServiceCompositionMetadata Draw();
        void PostDraw();
        Type GetBackingInterfaceType();
    }
}
