using System;
using System.Collections.Generic;

using WarnerEngine.Services;

namespace WarnerEngine.Lib
{
    public abstract class Interaction<TActor, TReceiver, TAccumulator> : BaseInteraction where TActor : class where TReceiver : class
    {
        protected TActor actor;
        protected TReceiver receiver;

        protected abstract TAccumulator InitialAccumulatorValue { get; }

        protected override void ProcessActionsImplementation()
        {
            TAccumulator accumulator = InitialAccumulatorValue;
            DisposableList<TActor> actors = GameService.GetService<IInteractionService>().GetCachedEntities<TActor>();
            for (int a = 0; a < actors.Count; a++)
            {
                TActor actor = actors[a];
                if (!CanPerformAction(actor, accumulator))
                {
                    continue;
                }
                // We don't need the list of receivers until at least one actor can perform the action
                DisposableList<TReceiver> receivers = GameService.GetService<IInteractionService>().GetCachedEntities<TReceiver>();
                for (int r = 0; r < receivers.Count; r++)
                {
                    TReceiver receiver = receivers[r];
                    if (actor == receiver)
                    {
                        continue;
                    }
                    accumulator = PerformAction(actor, receiver, accumulator);
                }
            }
        }

        protected abstract TAccumulator PerformAction(TActor Actor, TReceiver Receiver, TAccumulator Accumulator);

        protected abstract bool CanPerformAction(TActor Actor, TAccumulator Accumulator);

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
