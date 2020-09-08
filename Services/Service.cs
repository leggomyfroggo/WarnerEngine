﻿using System;
using System.Collections.Generic;

namespace WarnerEngine.Services
{
    public abstract class Service : IService
    {
        public abstract HashSet<Type> GetDependencies();
        public abstract void Initialize();
        public virtual void PreDraw(float DT) { }
        public virtual ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }
        public virtual void PostDraw() { }
        public abstract Type GetBackingInterfaceType();
    }
}
