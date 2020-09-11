using System;

namespace WarnerEngine.Lib
{
    public abstract class BaseInteraction

    {
        public void ProcessActions()
        {
            ProcessActionsImplementation();
        }

        protected abstract void ProcessActionsImplementation();

        public abstract Type GetActorType();

        public abstract Type GetReceiverType();
    }
}
