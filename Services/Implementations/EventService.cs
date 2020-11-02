using System;
using System.Collections.Generic;

namespace WarnerEngine.Services.Implementations
{
    public class EventService : IEventService
    {
        private Dictionary<string, List<Action<object>>> events;

        public HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>();
        }

        public void Initialize()
        {
            events = new Dictionary<string, List<Action<object>>>();
        }

        public void PreDraw(float DT) { }

        public ServiceCompositionMetadata Draw()
        {
            return ServiceCompositionMetadata.Empty;
        }

        public void PostDraw() { }

        public IEventService Subscribe(string EventKey, Action<object> EventCallback)
        {
            if (!events.ContainsKey(EventKey))
            {
                events[EventKey] = new List<Action<object>>();
            }
            events[EventKey].Add(EventCallback);
            return this;
        }

        public IEventService Unsubscribe(string EventKey, Action<object> EventCallback)
        {
            if (!events.ContainsKey(EventKey))
            {
                return this;
            }
            events[EventKey].Remove(EventCallback);
            return this;
        }

        public void Notify(string EventKey, object EventPayload = null)
        {
            if (!events.ContainsKey(EventKey))
            {
                return;
            }
            foreach (Action<object> callback in events[EventKey])
            {
                callback(EventPayload);
            }
        }

        public Type GetBackingInterfaceType()
        {
            return typeof(IEventService);
        }
    }
}
