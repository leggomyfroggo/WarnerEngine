using System;
using System.Collections.Generic;

using WarnerEngine.Services;

namespace WarnerEngine.Lib.ECS
{
    public abstract class ASystem
<TC1> : ISystem where TC1 : IComponent
    {
        public void Process()
        {
            ProcessImplementation(GameService.GetService<IECSService>().GetEntitiesWithComponent<TC1>());
        }

        public abstract void ProcessImplementation(IEnumerable<UInt64> Entities1);
    }

    public abstract class ASystem<TC1, TC2> : ISystem where TC1 : IComponent where TC2 : IComponent
    {
        public void Process()
        {
            ProcessImplementation(
                GameService.GetService<IECSService>().GetEntitiesWithComponent<TC1>(),
                GameService.GetService<IECSService>().GetEntitiesWithComponent<TC2>()
            );
        }

        public abstract void ProcessImplementation(IEnumerable<UInt64> EntityIDs1, IEnumerable<UInt64> EntityIDs2);
    }
}
