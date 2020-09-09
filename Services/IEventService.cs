using System;

namespace WarnerEngine.Services
{
    public interface IEventService : IService
    {
        void Notify(string EventKey, object EventPayload = null);
        void Subscribe(string EventKey, Action<object> EventCallback);
        void Unsubscribe(string EventKey, Action<object> EventCallback);
    }
}
