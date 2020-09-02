using System;

namespace ProjectWarnerShared.Services
{
    public interface IService
    {
        void PreDraw(float DT);
        ServiceCompositionMetadata Draw();
        void PostDraw();
        Type GetBackingInterfaceType();
    }
}
