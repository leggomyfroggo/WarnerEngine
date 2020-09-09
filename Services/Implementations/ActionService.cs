using System;
using System.Collections.Generic;

using WarnerEngine.Lib;

namespace WarnerEngine.Services.Implementations
{
    public class ActionService : IActionService
    {
        private List<BaseAction> actions;

        private Dictionary<Type, IEnumerable<object>> cachedEntities;

        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>() { };
        }

        public void Initialize()
        {
            actions = new List<BaseAction>();
            cachedEntities = new Dictionary<Type, IEnumerable<object>>();
        }

        public IActionService RegisterAction(BaseAction Action)
        {
            actions.Add(Action);
            return this;
        }

        public void PreDraw(float DT)
        {
            cachedEntities.Clear();
            foreach (BaseAction action in actions)
            {
                action.ProcessActions();
            }
        }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public List<T> GetCachedEntities<T>()
        {
            return GameService.GetService<ISceneService>().CurrentScene.GetEntitiesOfType<T>();
        }

        public Type GetBackingInterfaceType()
        {
            return typeof(IActionService);
        }
    }
}
