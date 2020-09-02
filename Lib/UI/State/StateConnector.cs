using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace ProjectWarnerShared.Lib.UI.State
{
    public abstract class StateConnector
    {
        private Dictionary<string, object> state;
        private Dictionary<string, List<System.Action<object, object>>> connections;

        public StateConnector()
        {
            state = new Dictionary<string, object>();            
            connections = new Dictionary<string, List<System.Action<object, object>>>();
        }

        public TState GetState<TState>(string Key)
        {
            if (!state.ContainsKey(Key))
            {
                return default(TState);
            }
            return (TState)state[Key];
        }

        public void SetState<TState>(string Key, TState Value)
        {
            TState oldState = GetState<TState>(Key);
            if (oldState != null && oldState.Equals(Value))
            {
                return;
            }
            state[Key] = Value;
            if (connections.ContainsKey(Key))
            {
                var keyConnections = connections[Key];
                foreach (var keyConnection in keyConnections)
                {
                    keyConnection(oldState, Value);
                }
            }
        }

        public void AddConnection<TState>(string Key, System.Action<TState, TState> Connection)
        {
            if (!connections.ContainsKey(Key))
            {
                connections[Key] = new List<System.Action<object, object>>();
            }
            connections[Key].Add((object oldValue, object newValue) => Connection((TState)oldValue, (TState)newValue));
        }

        // TODO: Make it easy to remove a connection
    }
}
