using WarnerEngine.Lib.Entities;

namespace WarnerEngine.Lib.Components.Combat
{
    public class ComboTracker : IPreDraw
    {
        private readonly AutoTween comboWindow;

        private bool didIncrementThisFrame;

        public int ComboCount { get; private set; }

        public ComboTracker(float ComboWindow)
        {
            comboWindow = new AutoTween(0, 1, ComboWindow);
            ComboCount = 1;
            didIncrementThisFrame = false;
        }

        public void PreDraw(float DT)
        {
            didIncrementThisFrame = false;
        }

        public void IncrementCombo()
        {
            if (didIncrementThisFrame)
            {
                return;
            }
            comboWindow.Update();
            if (!comboWindow.IsRunning)
            {
                ComboCount = 1;
            }
            else
            {
                ComboCount++;
            }
            didIncrementThisFrame = true;
            comboWindow.Start();
        }

        public void ResetCombo()
        {
            comboWindow.Cancel();
            ComboCount = 1;
        }
    }
}
