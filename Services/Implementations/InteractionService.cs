using System;
using System.Collections.Generic;

using WarnerEngine.Lib;

namespace WarnerEngine.Services.Implementations
{
    public class InteractionService : IInteractionService
    {
        private List<BaseInteraction> actions;

        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>() { };
        }

        public void Initialize()
        {
            actions = new List<BaseInteraction>();
        }

        public IInteractionService RegisterAction(BaseInteraction Action)
        {
            actions.Add(Action);
            return this;
        }

        public void PreDraw(float DT)
        {
            foreach (BaseInteraction action in actions)
            {
                action.ProcessActions();
            }
        }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public DisposableList<T> GetCachedEntities<T>()
        {
            return GameService.GetService<ISceneService>().CurrentScene.GetEntitiesOfType<T>();
        }

        public Type GetBackingInterfaceType()
        {
            return typeof(IInteractionService);
        }
    }
}
