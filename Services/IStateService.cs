using System;

namespace WarnerEngine.Services
{
    public interface IStateService : IService
    {
        int GetGlobalFrameCount();
        float GetGlobalGameTime();
        Random GetGlobalRandom();
        TValue GetState<TValue>(string Key);
        void IncrementGlobalFrameCount();
        bool IsFlagSet(string Flag);
        IStateService SetFlag(string Flag);
        void SetGlobalGameTime(float GameTime);
        IStateService SetState<TValue>(string Key, TValue Value);
        IStateService UnsetFlag(string Flag);
    }
}
