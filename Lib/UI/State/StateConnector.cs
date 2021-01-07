using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace WarnerEngine.Lib.UI.State
{
    public abstract class StateConnector
    {
        private Dictionary<string, object> state;
        private Dictionary<string, List<System.Action<object, object>>> connections;

        private UIRenderer uiRendererInstance;

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
            if (uiRendererInstance != null)
            {
                uiRendererInstance.ForceUpdate();
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

        public void ConnectUIRendererInstance(UIRenderer UIRendererInstance)
        {
            uiRendererInstance = UIRendererInstance;
        }

        // TODO: Make it easy to remove a connection
    }
}
