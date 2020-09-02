using System;
using System.Collections.Generic;

using WarnerEngine.Lib;

namespace WarnerEngine.Services
{
    public class ActionService : Service
    {
        private List<BaseAction> actions;

        private Dictionary<Type, IEnumerable<object>> cachedEntities;

        public ActionService()
        {
            actions = new List<BaseAction>();
            cachedEntities = new Dictionary<Type, IEnumerable<object>>();
        }

        public ActionService RegisterAction(BaseAction Action)
        {
            actions.Add(Action);
            return this;
        }

        public override void PreDraw(float DT)
        {
            cachedEntities.Clear();
            foreach (BaseAction action in actions)
            {
                action.ProcessActions();
            }
        }

        public List<T> GetCachedEntities<T>()
        {
            return GameService.GetService<SceneService>().CurrentScene.GetEntitiesOfType<T>();
        }

        public override Type GetBackingInterfaceType()
        {
            return typeof(ActionService);
        }
    }
}
