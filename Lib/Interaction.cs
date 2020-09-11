using System;
using System.Collections.Generic;

using WarnerEngine.Services;

namespace WarnerEngine.Lib
{
    public abstract class Interaction<TActor, TReceiver> : BaseInteraction where TActor : class where TReceiver : class
    {
        protected TActor actor;
        protected TReceiver receiver;

        protected override void ProcessActionsImplementation()
        {
            List<TActor> actors = GameService.GetService<IInteractionService>().GetCachedEntities<TActor>();
            foreach (TActor actor in actors)
            {
                if (!CanPerformAction(actor))
                {
                    continue;
                }
                // We don't need the list of receivers until at least one actor can perform the action
                List<TReceiver> receivers = GameService.GetService<IInteractionService>().GetCachedEntities<TReceiver>();
                foreach (TReceiver receiver in receivers)
                {
                    if (actor == receiver)
                    {
                        continue;
                    }
                    PerformAction(actor, receiver);
                }
            }
        }

        protected abstract void PerformAction(TActor Actor, TReceiver Receiver);

        protected abstract bool CanPerformAction(TActor Actor);

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
