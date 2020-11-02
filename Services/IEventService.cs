using System;

namespace WarnerEngine.Services
{
    public interface IEventService : IService
    {
        void Notify(string EventKey, object EventPayload = null);
        IEventService Subscribe(string EventKey, Action<object> EventCallback);
        IEventService Unsubscribe(string EventKey, Action<object> EventCallback);
    }
}
