using System;
using System.Collections.Generic;

using WarnerEngine.Lib;

namespace WarnerEngine.Services
{
    public interface IInteractionService : IService
    {
        List<T> GetCachedEntities<T>();
        IInteractionService RegisterAction(BaseInteraction Action);
    }
}
