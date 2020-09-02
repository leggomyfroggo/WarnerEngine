using System;
using System.Collections.Generic;

namespace ProjectWarnerShared.Services
{
    public class StateService : Service
    {
        private const string GLOBAL_RANDOM_KEY = "global_random";
        private const string GLOBAL_GAME_TIME_KEY = "global_game_time";
        private const string GLOBAL_FRAME_COUNT_KEY = "global_frame_count";
        private Dictionary<string, object> state;

        public StateService()
        {
            state = new Dictionary<string, object>();
            SetState(GLOBAL_RANDOM_KEY, new Random());
            SetState(GLOBAL_GAME_TIME_KEY, 0f);
            SetState(GLOBAL_FRAME_COUNT_KEY, 0);
        }

        public StateService SetState<TValue>(string Key, TValue Value)
        {
            state[Key] = Value;
            return this;
        }

        public TValue GetState<TValue>(string Key)
        {
            return (TValue)state[Key];
        }

        public Random GetGlobalRandom()
        {
            return GetState<Random>(GLOBAL_RANDOM_KEY);
        }

        public void SetGlobalGameTime(float GameTime)
        {
            SetState(GLOBAL_GAME_TIME_KEY, GameTime);
        }

        public float GetGlobalGameTime()
        {
            return GetState<float>(GLOBAL_GAME_TIME_KEY);
        }

        public void IncrementGlobalFrameCount()
        {
            SetState(GLOBAL_FRAME_COUNT_KEY, GetGlobalFrameCount() + 1);
        }

        public int GetGlobalFrameCount()
        {
            return GetState<int>(GLOBAL_FRAME_COUNT_KEY);
        }

        public StateService SetFlag(string Flag)
        {
            SetState(Flag, true);
            return this;
        }

        public StateService UnsetFlag(string Flag)
        {
            SetState(Flag, false);
            return this;
        }

        public bool IsFlagSet(string Flag)
        {
            if (!state.ContainsKey(Flag))
            {
                return false;
            }
            return (bool)state[Flag];
        }

        public override Type GetBackingInterfaceType()
        {
            return typeof(StateService);
        }
    }
}
