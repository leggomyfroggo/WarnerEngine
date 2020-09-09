using System;
using System.Collections.Generic;

using WarnerEngine.Services;

namespace WarnerEngine.Lib
{
    public abstract class Action<TActor, TReceiver> : BaseAction where TActor : class where TReceiver : class
    {
        protected TActor actor;
        protected TReceiver receiver;

        protected override void ProcessActionsImplementation()
        {
            List<TActor> actors = GameService.GetService<IActionService>().GetCachedEntities<TActor>();
            foreach (TActor actor in actors)
            {
                if (!CanPerformActionImplementation(actor))
                {
                    continue;
                }
                // We don't need the list of receivers until at least one actor can perform the action
                List<TReceiver> receivers = GameService.GetService<IActionService>().GetCachedEntities<TReceiver>();
                foreach (TReceiver receiver in receivers)
                {
                    if (actor == receiver)
                    {
                        continue;
                    }
                    PerformActionImplementation(actor, receiver);
                }
            }
        }

        protected abstract void PerformActionImplementation(TActor Actor, TReceiver Receiver);

        protected abstract bool CanPerformActionImplementation(TActor Actor);

        public override Type GetActorType()
        {
            return typeof(TActor);
        }

        public override Type GetReceiverType()
        {
            return typeof(TReceiver);
        }
    }
}
