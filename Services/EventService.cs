using System;
using System.Collections.Generic;

namespace WarnerEngine.Services
{
    public class EventService : Service
    {
        private Dictionary<string, List<Action<object>>> events;

        public override HashSet<Type> GetDependencies()
        {
            return new HashSet<Type>();
        }

        public override void Initialize()
        {
            events = new Dictionary<string, List<Action<object>>>();
        }

        public void Subscribe(string EventKey, Action<object> EventCallback)
        {
            if (!events.ContainsKey(EventKey))
            {
                events[EventKey] = new List<Action<object>>();
            }
            events[EventKey].Add(EventCallback);
        }

        public void Unsubscribe(string EventKey, Action<object> EventCallback)
        {
            if (!events.ContainsKey(EventKey))
            {
                return;
            }
            events[EventKey].Remove(EventCallback);
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

        public override Type GetBackingInterfaceType()
        {
            return typeof(EventService);
        }
    }
}
