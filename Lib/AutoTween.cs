using WarnerEngine.Services;

namespace WarnerEngine.Lib
{
    public class AutoTween
    {
        private float start;
        private float end;
        public float Duration { get; private set; }

        public bool IsRunning { get; private set; }
        private float startTime;

        public AutoTween(float Start, float End, float Duration)
        {
            start = Start;
            end = End;
            this.Duration = Duration;

            IsRunning = false;
        }

        public AutoTween Start()
        {
            startTime = GameService.GetService<IStateService>().GetGlobalGameTime();
            IsRunning = true;
            return this;
        }

        public AutoTween Cancel()
        {
            IsRunning = false;
            return this;
        }

        public AutoTween Update()
        {
            GetTween();
            return this;
        }

        public float GetTween()
        {
            if (!IsRunning)
            {
                return end;
            }
            float currentTime = GameService.GetService<IStateService>().GetGlobalGameTime();
            float elapsedTime = currentTime - startTime;
            if (elapsedTime >= Duration)
            {
                IsRunning = false;
                return end;
            }
            return start + (end - start) * (elapsedTime / Duration);
        }

        public float GetElapsedDuration()
        {
            if (!IsRunning)
            {
                return 0;
            }
            float currentTime = GameService.GetService<IStateService>().GetGlobalGameTime();
            return currentTime - startTime;
        }
    }
}
