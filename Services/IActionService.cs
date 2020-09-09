using System;
using System.Collections.Generic;

using WarnerEngine.Lib;

namespace WarnerEngine.Services
{
    public interface IActionService : IService
    {
        List<T> GetCachedEntities<T>();
        IActionService RegisterAction(BaseAction Action);
    }
}
